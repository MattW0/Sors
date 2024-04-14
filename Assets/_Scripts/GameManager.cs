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
    private Market _market;
    private EndScreen _endScreen;
    private BoardManager _boardManager;

    private GameOptions _gameOptions;
    public bool isSinglePlayer = false;
    public static event Action<GameOptions> OnGameStart;

    [Header("Game state")]
    [SyncVar] public int turnNumber;
    public Dictionary<NetworkIdentity, PlayerManager> players = new();
    private List<PlayerManager> _loosingPlayers = new();

    [Header("Available cards")]
    public ScriptableCard[] startEntities;
    public ScriptableCard[] creatureCardsDb;
    public ScriptableCard[] moneyCardsDb;
    public ScriptableCard[] technologyCardsDb;
    private List<int> _availableCreatureIds = new();
    private List<int> _availableTechnologyIds = new();
    private Sprite[] _moneySprites;
    private int _nbMoneyTiles;
    private int _nbTechnologyTiles;
    private int _nbCreatureTiles;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject technologyCardPrefab;
    [SerializeField] private GameObject creatureEntityPrefab;
    [SerializeField] private GameObject technologyEntityPrefab;

    // Caching all gameObjects of cards in game
    private static Dictionary<int, GameObject> CardsCache { get; set; } = new();
    public static GameObject GetCardObject(int goID) { return CardsCache[goID]; }
    // convert cardInfo (goIDs) to cached objects
    public static List<GameObject> CardInfosToGameObjects(List<CardInfo> cards){
        return cards.Select(card => GetCardObject(card.goID)).ToList();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        TurnManager.OnPlayerDies += PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
        
        LoadCards();
    }

    private void LoadCards()
    {
        _moneySprites = Resources.LoadAll<Sprite>("Sprites/Money/");
        startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");

        // Databases of generated cards
        creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
        technologyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/TechnologyCards/");
        moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");

        var msg = " --- Available cards: --- \n" +
                  $"Creature cards: {creatureCardsDb.Length}\n" +
                  $"Money cards: {moneyCardsDb.Length}\n" +
                  $"Develop cards: {technologyCardsDb.Length}";
        print(msg);
    }

    #region Setup
    
    public void GameSetup(GameOptions options)
    {
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _market = Market.Instance;
        _endScreen = EndScreen.Instance;

        print(" --- Game starting --- \n" + options.ToString());
        _gameOptions = options;

        // initialHandSize = options.FullHand ? _gameOptions.initialDeckSize : _gameOptions.InitialHandSize;
        isSinglePlayer = options.SinglePlayer;

        InitPlayers();

        if(string.IsNullOrWhiteSpace(options.StateFile)){
            // Normal game setup
            MarketSetup();
            foreach (var player in players.Values) SpawnPlayerDeck(player);
            OnGameStart?.Invoke(_gameOptions);
        } else {
            // Start game from state file
            gameObject.GetComponent<GameStateLoader>().LoadGameState(options.StateFile);
        }
    }

    private void InitPlayers()
    {
        var playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (var player in playerManagers)
        {
            player.RpcInitPlayer();

            // TODO: Most things break if we actually want an AI opponent
            if(!player.isAI) players.Add(player.GetComponent<NetworkIdentity>(), player);
            
            // Player stats
            player.PlayerName = player.gameObject.name; // Object name is set after instantiation in NetworkManager
            player.Health = _gameOptions.startHealth;
            player.Score = _gameOptions.startScore;
            
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

    private void MarketSetup()
    {
        _market.RpcSetPlayer();

        // Money
        var moneyCards = new CardInfo[_nbMoneyTiles];
        for (var i = 0; i < _nbMoneyTiles; i++) moneyCards[i] = new CardInfo(moneyCardsDb[i]);
        _market.RpcSetMoneyTiles(moneyCards);

        // Technologies
        var technologies = new CardInfo[_nbTechnologyTiles];
        for (var i = 0; i < _nbTechnologyTiles; i++) technologies[i] = GetNewTechnologyFromDb();
        _market.RpcSetTechnologyTiles(technologies);

        // Creatures
        var creatures = new CardInfo[_nbCreatureTiles];
        for (var i = 0; i < _nbCreatureTiles; i++) creatures[i] = GetNewCreatureFromDb();
        _market.RpcSetCreatureTiles(creatures);
    }

    private void SpawnPlayerDeck(PlayerManager player)
    {
        List<GameObject> startCards = new();
        // Only paper money currently
        for (var i = 0; i < _gameOptions.initialDeckSize - _gameOptions.initialEntities; i++){
            var scriptableCard = moneyCardsDb[0];
            startCards.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        for (var i = 0; i < _gameOptions.initialEntities; i++){
            var scriptableCard = startEntities[i];
            startCards.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        player.RpcShowSpawnedCards(startCards, CardLocation.Deck, false);
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
        var cardInfo = IntitializeCardOnClients(cardObject, scriptableCard, instanceID);

        // Must always be done on server side (in TurnManager except here after spawning)
        AddCardToPlayerCollection(player, cardInfo, destination);

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

    private CardInfo IntitializeCardOnClients(GameObject cardObject, ScriptableCard scriptableCard, int instanceID){
        // Client RPC : Init card UI and disable gameObject (cause not yet on UI layer)
        var cardInfo = new CardInfo(scriptableCard, instanceID);
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);

        return cardInfo;
    }

    private void AddCardToPlayerCollection(PlayerManager owner, CardInfo cardInfo, CardLocation destination){
        if (destination == CardLocation.Deck) owner.deck.Add(cardInfo);
        else if(destination == CardLocation.Discard) owner.discard.Add(cardInfo);
        else if(destination == CardLocation.Hand) owner.hand.Add(cardInfo);
    }

    public BattleZoneEntity SpawnFieldEntity(PlayerManager owner, GameObject card)
    {
        var cardInfo = card.GetComponent<CardStats>().cardInfo;

        // Entity prefab depending on type
        GameObject entityObject = cardInfo.type switch
        {
            CardType.Creature => Instantiate(creatureEntityPrefab) as GameObject,
            CardType.Technology => Instantiate(technologyEntityPrefab) as GameObject,
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

    private void PlayerDies(PlayerManager player)
    {
        _loosingPlayers.Add(player);
    }

    public void EndGame()
    {
        foreach (var player in players.Values)
        {
            var health = player.Health;
            _endScreen.RpcSetFinalScore(player, health, 0);
        }
        
        // Both players die -> Draw
        if (_loosingPlayers.Count == players.Count)
        {
            _endScreen.RpcGameIsDraw();
            return;
        }
        
        _endScreen.RpcIsLooser(_loosingPlayers[0]);
    }

    #endregion

    #region Utils
    public void StartGame() => OnGameStart?.Invoke(_gameOptions);

    public CardInfo GetNewTechnologyFromDb()
    {
        if(_availableTechnologyIds.Count == 0){
            // Random order of ids -> pop first element for random card
            _availableTechnologyIds = Enumerable.Range(0, technologyCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableTechnologyIds[0];
        _availableTechnologyIds.RemoveAt(0);
        return new CardInfo(technologyCardsDb[id]);
    }

    public CardInfo GetNewCreatureFromDb()
    {
        if(_availableCreatureIds.Count == 0){
            // Random order of ids -> pop first element for random card
            _availableCreatureIds = Enumerable.Range(0, creatureCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableCreatureIds[0];
        _availableCreatureIds.RemoveAt(0);
        return new CardInfo(creatureCardsDb[id]);
    }

    public void SetNumberOfMarketTiles(int moneyTiles, int technologies, int creatures)
    {
        _nbMoneyTiles = moneyTiles;
        _nbTechnologyTiles = technologies;
        _nbCreatureTiles = creatures;
    }

    [Server]
    private void SkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();

    public PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Values.FirstOrDefault(p => p != player);
    }
    #endregion

    private void OnDestroy()
    {
        TurnManager.OnPlayerDies -= PlayerDies;
        SorsNetworkManager.OnAllPlayersReady -= GameSetup;
    }
}
