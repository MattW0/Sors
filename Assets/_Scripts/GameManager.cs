using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour {
    [Header("For Coding")]
    public bool test;
    public bool singlePlayer;
    public bool animations;

    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    private Kingdom _kingdom;
    private EndScreen _endScreen;

    public static event Action<int> OnGameStart;
    public static event Action<PlayerManager, BattleZoneEntity, GameObject> OnEntitySpawned; 

    [Header("Game state")]
    [SyncVar] public int turnNb;
    // public int numberPlayers = 2;
    public Dictionary<PlayerManager, NetworkIdentity> players = new();
    private List<PlayerManager> _loosingPlayers = new();

    [Header("Turn specifics")]
    public int nbPhasesToChose = 2;
    public int nbCardDraw = 2;
    public int nbDiscard = 1;
    public int turnCash = 0;
    public int turnDeploys = 1; 
    public int turnRecruits = 1;

    [Header("Game start settings")]
    [SerializeField] private int nbKingdomCards = 12;
    public int initialDeckSize = 10;
    public int nbCreatures = 2;
    public int initialHandSize = 4;
    public int startHealth = 30;
    public int startScore = 0;

    [Header("Available cards")]
    public ScriptableCard[] startCards;
    public ScriptableCard[] creatureCardsDb;
    private List<int> _cardsIdCache = new();
    public ScriptableCard[] moneyCards;
    private Sprite[] _moneySprites;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject discardPanelPrefab;
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject entityObjectPrefab;

    // Caching all gameObjects of cards in game
    private Dictionary<string, GameObject> Cache { get; set; }
    public GameObject GetCardObject(string goID) { return Cache[goID]; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        startCards = Resources.LoadAll<ScriptableCard>("StartCards/");
        creatureCardsDb = Resources.LoadAll<ScriptableCard>("creatureCards/");
        moneyCards = Resources.LoadAll<ScriptableCard>("MoneyCards/");
        _moneySprites = Resources.LoadAll<Sprite>("Sprites/Money/");

        Cache = new Dictionary<string, GameObject>();
        TurnManager.OnPlayerDies += PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
    }

    public void GameSetup(int nbPlayers)
    {
        singlePlayer = nbPlayers == 1;

        _turnManager = TurnManager.Instance;
        _kingdom = Kingdom.Instance;
        _endScreen = EndScreen.Instance;
        
        KingdomSetup();
        PlayerSetup();
        
        OnGameStart?.Invoke(nbPlayers);
    }

    #region Setup
    
    private void KingdomSetup(){
        var kingdomCards = new CardInfo[nbKingdomCards];
        
        for (var i = 0; i < nbKingdomCards; i++) kingdomCards[i] = GetNewCardFromDb();
        _kingdom.RpcSetKingdomCards(kingdomCards);
    }

    private CardInfo GetNewCardFromDb(){

        // Get new random card
        var id = Random.Range(0, creatureCardsDb.Length);
        while (_cardsIdCache.Contains(id)) id = Random.Range(0, creatureCardsDb.Length);

        _cardsIdCache.Add(id);
        var card = creatureCardsDb[id];

        return new CardInfo(card);
    }

    private void PlayerSetup(){
        
        var playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (var player in playerManagers)
        {
            var playerNetworkId = player.GetComponent<NetworkIdentity>();
            players.Add(player, playerNetworkId);

            // UI
            player.PlayerName = player.PlayerName; // To update info in network
            player.Health = startHealth;
            player.Score = startScore;
            player.Cash = turnCash;
            player.Recruits = turnRecruits;

            // Cards
            SpawnPlayerDeck(player);
            player.cards.deck.Shuffle();

            player.DrawInitialHand(initialHandSize);
        }
    }

    #endregion

    #region Spawning
    
    private void SpawnPlayerDeck(PlayerManager playerManager){
        // Coppers
        for (var i = 0; i < initialDeckSize - nbCreatures; i++){
            var moneyCard = moneyCards[2]; // Only copper right now
            var cardObject = Instantiate(moneyCardPrefab);
            SpawnCacheAndMoveCard(playerManager, cardObject, moneyCard, CardLocations.Deck);
        }

        // Other start cards
        for (var i = 0; i < nbCreatures; i++){
            var creatureCard = startCards[i]; // Special creatures 'A' & 'B' 
            var cardObject = Instantiate(creatureCardPrefab);
            SpawnCacheAndMoveCard(playerManager, cardObject, creatureCard, CardLocations.Deck);
        }
    }
    
    private void SpawnCacheAndMoveCard(PlayerManager owner, GameObject cardObject,
                                       ScriptableCard scriptableCard, CardLocations destination){

        var instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        Cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);

        var cardInfo = new CardInfo(scriptableCard, instanceID);
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);
        
        switch (destination)
        {
            case CardLocations.Deck:
                owner.cards.deck.Add(cardInfo);
                break;
            case CardLocations.Discard:
                owner.cards.discard.Add(cardInfo);
                break;
            case CardLocations.Hand:
                owner.cards.hand.Add(cardInfo);
                break;
            case CardLocations.Spawned:
                break;
            case CardLocations.PlayZone:
                break;
            case CardLocations.MoneyZone:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destination), destination, null);
        }
        owner.RpcMoveCard(cardObject, CardLocations.Spawned, destination);
    }

    public void SpawnCreature(PlayerManager player, CardInfo cardInfo){

        _kingdom.RpcReplaceCard(cardInfo.title, GetNewCardFromDb());

        var scriptableCard = Resources.Load<ScriptableCard>("creatureCards/" + cardInfo.title);
        var cardObject = Instantiate(creatureCardPrefab);
        SpawnCacheAndMoveCard(player, cardObject, scriptableCard, CardLocations.Discard);
    }

    public void SpawnFieldEntity(PlayerManager owner, GameObject card, 
        CardInfo cardInfo, int cardHolder)
    {
        var entityObject = Instantiate(entityObjectPrefab);
        NetworkServer.Spawn(entityObject, connectionToClient);
        entityObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);
        
        var opponent = GetOpponent(owner);
        var entity = entityObject.GetComponent<BattleZoneEntity>();
        entity.RpcSpawnEntity(owner, opponent, cardInfo, cardHolder);
        
        OnEntitySpawned?.Invoke(owner, entity, card);
    }
    #endregion

    #region Ending

    private void PlayerDies(PlayerManager player)
    {
        _loosingPlayers.Add(player);
    }

    public void EndGame()
    {
        foreach (var player in players.Keys)
        {
            var health = _turnManager.GetHealth(player);
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

    private PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Keys.FirstOrDefault(p => p != player);
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerDies -= PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
    }
}

public enum CardLocations{
    Spawned,
    Deck,
    Hand,
    PlayZone,
    MoneyZone,
    Discard
}
