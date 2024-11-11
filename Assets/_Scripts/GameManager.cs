using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour {
    
    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    private UIManager _uiManager;
    private Market _market;
    private GameOptions _gameOptions;
    public bool isSinglePlayer = false;
    public static event Action<GameOptions> OnGameStart;

    [Header("Game state")]
    [SyncVar] public int turnNumber;
    public Dictionary<NetworkIdentity, PlayerManager> players = new();
    private List<PlayerManager> _loosingPlayers = new();
    private List<PlayerManager> _winningPlayers = new();
    
    [Header("Spawnable Prefabs")]
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject technologyCardPrefab;
    [SerializeField] private GameObject creatureEntityPrefab;
    [SerializeField] private GameObject technologyEntityPrefab;

    // Caching all gameObjects of cards in game
    private static Dictionary<int, GameObject> CardsCache { get; set; } = new();
    public static GameObject GetCardObject(int goID) { return CardsCache[goID]; }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;

        Sors.Lan.SorsNetworkManager.OnAllPlayersReady += GameSetup;
        SorsSteamNetworkManager.OnAllPlayersReady += GameSetup;
    }

    #region Setup
    
    public void GameSetup(GameOptions options)
    {
        _turnManager = TurnManager.Instance;
        _market = Market.Instance;
        _uiManager = UIManager.Instance;

        print(" --- Game starting --- \n" + options.ToString());
        _gameOptions = options;
        // isSinglePlayer = options.SinglePlayer;

        InitPlayers();

        if(string.IsNullOrWhiteSpace(options.StateFile)){
            // Normal game setup
            _market.RpcInitializeMarket();
            foreach (var player in players.Values) SpawnPlayerDeck(player);
            OnGameStart?.Invoke(_gameOptions);
        } else {
            // Start game from state file
            gameObject.GetComponent<GameStateLoader>().LoadGameState(options.StateFile);
        }
    }

    private void InitPlayers()
    {
        var playerManagers = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        foreach (var player in playerManagers)
        {
            player.RpcInitPlayer();

            // TODO: Most things break if we actually want an AI opponent
            if(!player.isAI) players.Add(player.GetComponent<NetworkIdentity>(), player);
            
            // Player stats
            player.PlayerName = player.gameObject.name; // Object name is set after instantiation in NetworkManager
            player.Health = _gameOptions.startHealth;
            player.Score = 0;
            
            // Turn stats
            player.Cash = 0;
            player.Buys = 0;
            player.Plays = 0;
            player.Prevails = 0;
        }

        if(_gameOptions.SkipCardSpawnAnimations) {
            // Needs to be done on all clients
            playerManagers[0].RpcSkipCardSpawnAnimations();
            // And on server
            SkipCardSpawnAnimations();
        }
    }

    private void SpawnPlayerDeck(PlayerManager player)
    {
        List<GameObject> startingDeck = new();

        // Only paper money currently
        var startMoney = _market.GetStartMoneyCard();
        for (var i = 0; i < _gameOptions.initialDeckSize - _gameOptions.initialEntities; i++){
            var scriptableCard = startMoney;
            startingDeck.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        var startEntities = _market.GetStartEntities();
        for (var i = 0; i < _gameOptions.initialEntities; i++){
            var scriptableCard = startEntities[i];
            startingDeck.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        player.RpcShowSpawnedCards(startingDeck, CardLocation.Deck, false);
    }

    #endregion

    #region Spawning
    public GameObject SpawnCardAndAddToCollection(PlayerManager player, ScriptableCard scriptableCard, CardLocation destination)
    {
        // print($"Spawning card {scriptableCard.title}, type : {scriptableCard.type}");

        // Card object prefab depending on type
        var cardObject = scriptableCard.type switch
        {
            CardType.Money => Instantiate(moneyCardPrefab) as GameObject,
            CardType.Creature => Instantiate(creatureCardPrefab) as GameObject,
            CardType.Technology => Instantiate(technologyCardPrefab) as GameObject,
            CardType.None => null,
            _ => null
        };

        // Assign client authority and put in cache
        var instanceID = SpawnAndCacheCard(player, cardObject, scriptableCard);

        // RPC: Setup gameobject and card UI
        var cardStats = IntitializeCardOnClients(cardObject, scriptableCard, instanceID);

        // Must always be done on server side (in TurnManager except here after spawning)
        AddCardToPlayerCollection(player, cardStats, destination);

        return cardObject;
    }

    public void PlayerGainCard(PlayerManager player, CardInfo card)
    {
        // Load scriptable
        var pathPrefix = card.type switch {
            CardType.Money => "Cards/MoneyCards/",
            CardType.Creature => "Cards/CreatureCards/",
            CardType.Technology => "Cards/TechnologyCards/",
            _ => ""
        };
        var scriptableCard = Resources.Load<ScriptableCard>(pathPrefix + card.resourceName);

        // Resolve card gain
        var cardObject = SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Discard);
        player.RpcShowSpawnedCard(cardObject, CardLocation.Discard);
    }
    
    private int SpawnAndCacheCard(PlayerManager owner, GameObject cardObject, ScriptableCard scriptableCard){
        // Using the unique gameObject instance ID ()
        var instanceID = cardObject.GetInstanceID();
        cardObject.name = instanceID.ToString();
        CardsCache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(owner.connectionToClient);

        return instanceID;
    }

    private CardStats IntitializeCardOnClients(GameObject cardObject, ScriptableCard scriptableCard, int instanceID){
        // Client RPC : Init card UI and disable gameObject (cause not yet on UI layer)
        cardObject.GetComponent<CardStats>().RpcSetCardStats(new CardInfo(scriptableCard, instanceID));

        return cardObject.GetComponent<CardStats>();
    }

    private void AddCardToPlayerCollection(PlayerManager owner, CardStats card, CardLocation destination){
        if (destination == CardLocation.Deck) owner.deck.Add(card);
        else if(destination == CardLocation.Discard) owner.discard.Add(card);
        else if(destination == CardLocation.Hand) owner.hand.Add(card);
    }

    public BattleZoneEntity SpawnFieldEntity(PlayerManager owner, CardInfo cardInfo)
    {
        // Entity prefab depending on type
        GameObject entityObject = cardInfo.type switch
        {
            CardType.Creature => Instantiate(creatureEntityPrefab),
            CardType.Technology => Instantiate(technologyEntityPrefab),
            _ => null
        };
        
        // Assign authority
        NetworkServer.Spawn(entityObject, connectionToClient);
        entityObject.GetComponent<NetworkIdentity>().AssignClientAuthority(owner.connectionToClient);
        var entity = entityObject.GetComponent<BattleZoneEntity>();

        // Intitialize entity on clients
        entity.RpcInitializeEntity(owner, cardInfo);
        
        return entity;
    }
    #endregion

    #region Ending
    internal void PlayerIsDead(PlayerManager player){
        if(_loosingPlayers.Contains(player)) return;
        _loosingPlayers.Add(player);
    }
    internal void PlayerHasWinScore(PlayerManager player) {
        if(_winningPlayers.Contains(player)) return;
        _winningPlayers.Add(player);
    }
    internal void EndGame()
    {
        foreach (var player in players.Values){
            _uiManager.RpcSetPlayerScore(player, player.Health, player.Score);
        }

        PlayerManager winner = null;
        if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 0) {} // Should never happen
        else if(_winningPlayers.Count == 1 && _loosingPlayers.Count == 0){
            winner = _winningPlayers[0];
        } else if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 1){
            winner = _turnManager.GetOpponentPlayer(_loosingPlayers[0]);
        } else if (_winningPlayers.Count == 1 && _loosingPlayers.Count == 1){
            if (_winningPlayers[0] != _loosingPlayers[0]) winner = _winningPlayers[0];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 2 && _loosingPlayers.Count == 0){
            if (_winningPlayers[0].Score > _winningPlayers[1].Score) winner = _winningPlayers[0];
            else if (_winningPlayers[0].Score < _winningPlayers[1].Score) winner = _winningPlayers[1];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 2){
            if (_loosingPlayers[0].Health > _loosingPlayers[1].Health) winner =_loosingPlayers[0];
            else if (_loosingPlayers[0].Health < _loosingPlayers[1].Health) winner = _loosingPlayers[1];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 2 && _loosingPlayers.Count == 1){
            if(_winningPlayers[0] == _loosingPlayers[0]) winner = _winningPlayers[1];
            else winner = _winningPlayers[0];
        } else if (_winningPlayers.Count == 1 && _loosingPlayers.Count == 2){
            if(_winningPlayers[0] == _loosingPlayers[0]) winner = _loosingPlayers[0];
            else winner = _winningPlayers[1];
        } else {
            if (_winningPlayers[0].Score > _winningPlayers[1].Score) winner = _winningPlayers[0];
            else if (_winningPlayers[0].Score < _winningPlayers[1].Score) winner = _winningPlayers[1];
            else if (_loosingPlayers[0].Health > _loosingPlayers[1].Health) winner =_loosingPlayers[0];
            else if (_loosingPlayers[0].Health < _loosingPlayers[1].Health) winner = _loosingPlayers[1];
        }

        if(winner) _uiManager.RpcSetGameWinner(winner);
        else _uiManager.RpcSetDraw();
    }

    #endregion

    #region Utils
    public void StartGame() => OnGameStart?.Invoke(_gameOptions);

    [Server]
    private void SkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();

    public PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Values.FirstOrDefault(p => p != player);
    }
    #endregion

    private void OnDestroy()
    {
        SorsSteamNetworkManager.OnAllPlayersReady -= GameSetup;
        Sors.Lan.SorsNetworkManager.OnAllPlayersReady -= GameSetup;
    }
}
