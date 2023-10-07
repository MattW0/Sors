using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour {
    
    [Header("For Coding")]
    public bool singlePlayer;
    public bool animations = true;
    public bool cardSpawnAnimations = false;

    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    private Kingdom _kingdom;
    private EndScreen _endScreen;
    private BoardManager _boardManager;

    public static event Action<int> OnGameStart;

    [Header("Game state")]
    [SyncVar] public int turnNb;
    public Dictionary<PlayerManager, NetworkIdentity> players = new();
    private List<PlayerManager> _loosingPlayers = new();

    [Header("Game start settings")]
    private int _nbMoneyTiles;
    private int _nbDevelopTiles;
    private int _nbCreatureTiles;
    [SerializeField] private int initialDeckSize = 10;
    [SerializeField] private int initialCreatures = 3;
    [SerializeField] private int initialDevelopments = 2;
    [SerializeField] private int initialHandSize = 6;
    public int startHealth = 10;
    public int startScore = 0;

    [Header("Turn specifics")]
    public int nbPhasesToChose;
    [SerializeField] public int fixCardDraw = 2;
    [SerializeField] public int phaseCardDraw = 2;
    [SerializeField] public int nbDiscard = 1;
    public int turnCash = 0;
    public int turnInvents = 1; 
    public int turnDevelops = 1; 
    public int turnDeploys = 1; 
    public int turnRecruits = 1;
    public int prevailOptionsToChoose = 1;

    [Header("Turn Boni")]
    public int extraDraw = 2;
    public int kingdomPriceReduction = 1;
    public int developPriceReduction = 1;
    public int prevailExtraOptions = 2;
    public int deployBonusDeploys = 1;


    [Header("Available cards")]
    public ScriptableCard[] startCreatures;
    public ScriptableCard[] startDevelopments;
    public ScriptableCard[] creatureCardsDb;
    public ScriptableCard[] moneyCardsDb;
    public ScriptableCard[] developCardsDb;
    private List<int> _availableCreatureIds = new();
    private List<int> _availableDevelopmentIds = new();
    private Sprite[] _moneySprites;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject developCardPrefab;
    [SerializeField] private GameObject creatureEntityPrefab;
    [SerializeField] private GameObject developmentEntityPrefab;

    // Caching all gameObjects of cards in game
    private static Dictionary<string, GameObject> Cache { get; set; } = new();
    public static GameObject GetCardObject(string goID) { return Cache[goID]; }
    // convert cardInfo (goIDs) to cached objects
    public static List<GameObject> CardInfosToGameObjects(List<CardInfo> cards){
        return cards.Select(card => GetCardObject(card.goID)).ToList();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        LoadCards();

        _moneySprites = Resources.LoadAll<Sprite>("Sprites/Money/");

        TurnManager.OnPlayerDies += PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
    }

    private void LoadCards(){
        startCreatures = Resources.LoadAll<ScriptableCard>("Cards/StartCards/Creatures/");
        startDevelopments = Resources.LoadAll<ScriptableCard>("Cards/StartCards/Developments/");

        // Databases of generated cards
        creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
        developCardsDb = Resources.LoadAll<ScriptableCard>("Cards/DevelopCards/");
        moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");

        var msg = " --- Available cards: --- \n" +
                  $"Creature cards: {creatureCardsDb.Length}\n" +
                  $"Money cards: {moneyCardsDb.Length}\n" +
                  $"Develop cards: {developCardsDb.Length}";
        print(msg);
    }

    public void GameSetup(GameOptions options){

        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _kingdom = Kingdom.Instance;
        _endScreen = EndScreen.Instance;

        print(" --- Game starting --- \n" + options.ToString());
        singlePlayer = options.NumberPlayers == 1;
        nbPhasesToChose = options.NumberPhases;
        initialHandSize = options.FullHand ? initialDeckSize : initialHandSize;
        cardSpawnAnimations = options.CardSpawnAnimations;

        if(! string.IsNullOrWhiteSpace(options.StateFile)) LoadGameState(options.StateFile);

        KingdomSetup();
        PlayerSetup();

        if(cardSpawnAnimations) StartCoroutine(PreGameWaitRoutine());
    }

    #region Setup
    private void LoadGameState(string fileName){
        var stateFile = Resources.Load<TextAsset>("GameStates/" + fileName);
        var state = stateFile.GetData<GameState>();

        // TODO: How to load json and convert string data ?
        // foreach (var key in stateFile){
        //     print(key);
        // }
    }
    
    private void KingdomSetup(){
        _kingdom.RpcSetPlayer();
        
        // Developments: right now only money
        var moneyCards = new CardInfo[_nbMoneyTiles];
        for (var i = 0; i < _nbMoneyTiles; i++) moneyCards[i] = new CardInfo(moneyCardsDb[i]);
        _kingdom.RpcSetMoneyTiles(moneyCards);

        var developCards = new CardInfo[_nbDevelopTiles];
        for (var i = 0; i < _nbDevelopTiles; i++) developCards[i] = GetNewDevelopmentFromDb();
        _kingdom.RpcSetDevelopmentTiles(developCards);

        // Recruits: Creatures
        var recruitCards = new CardInfo[_nbCreatureTiles];
        for (var i = 0; i < _nbCreatureTiles; i++) recruitCards[i] = GetNewCreatureFromDb();
        _kingdom.RpcSetRecruitTiles(recruitCards);
    }

    private CardInfo GetNewDevelopmentFromDb(){
        if(_availableDevelopmentIds.Count == 0) {
            // Random order of ids -> pop first element for random card
            _availableDevelopmentIds = Enumerable.Range(0, developCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableDevelopmentIds[0];
        _availableDevelopmentIds.RemoveAt(0);
        return new CardInfo(developCardsDb[id]);
    }

    private CardInfo GetNewCreatureFromDb(){
        if(_availableCreatureIds.Count == 0) {
            // Random order of ids -> pop first element for random card
            _availableCreatureIds = Enumerable.Range(0, creatureCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableCreatureIds[0];
        _availableCreatureIds.RemoveAt(0);
        return new CardInfo(creatureCardsDb[id]);
    }

    private void PlayerSetup(){
        
        var playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (var player in playerManagers)
        {
            var playerNetworkId = player.GetComponent<NetworkIdentity>();
            players.Add(player, playerNetworkId);
            player.RpcInitPlayer();

            // Player stats
            player.PlayerName = player.PlayerName; // To update info in network
            player.Health = startHealth;
            player.Score = startScore;
            
            // Turn stats
            player.Cash = turnCash;
            player.Invents = turnInvents;
            player.Develops = turnDevelops;
            player.Deploys = turnDeploys;
            player.Recruits = turnRecruits;

            // Cards
            SpawnPlayerDeck(player);

            if(!cardSpawnAnimations) {
                player.deck.Shuffle();
                player.DrawInitialHand(initialHandSize);
            } else {
                player.RpcResolveCardSpawn(cardSpawnAnimations);
            }
        }

        if(!cardSpawnAnimations) OnGameStart?.Invoke(players.Count);
    }

    #endregion

    #region Spawning
    
    private void SpawnPlayerDeck(PlayerManager playerManager){
        // Money
        for (var i = 0; i < initialDeckSize - initialCreatures - initialDevelopments; i++){
            var scriptableCard = moneyCardsDb[0]; // Only paper money right now
            var cardObject = Instantiate(moneyCardPrefab) as GameObject;
            var cardInfo = SpawnAndCacheCard(playerManager, cardObject, scriptableCard);
            MoveSpawnedCard(playerManager, cardObject, cardInfo, CardLocation.Deck);
        }

        // Creatures
        for (var i = 0; i < initialCreatures; i++){
            var scriptableCard = startCreatures[i]; // Special creatures 'Peasant', 'Worker'
            var cardObject = Instantiate(creatureCardPrefab) as GameObject;
            var cardInfo = SpawnAndCacheCard(playerManager, cardObject, scriptableCard);
            MoveSpawnedCard(playerManager, cardObject, cardInfo, CardLocation.Deck);
        }

        // Creatures
        for (var i = 0; i < initialDevelopments; i++){
            var scriptableCard = startDevelopments[i]; // Start Developments: 'A', 'B'
            var cardObject = Instantiate(developCardPrefab) as GameObject;
            var cardInfo = SpawnAndCacheCard(playerManager, cardObject, scriptableCard);
            MoveSpawnedCard(playerManager, cardObject, cardInfo, CardLocation.Deck);
        }
    }

    public void SpawnMoney(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/MoneyCards/" + card.hash + "_" + card.title);
        var cardObject = Instantiate(moneyCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Discard);
    }

    public void SpawnDevelopment(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/DevelopCards/" + card.title);
        var cardObject = Instantiate(developCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Discard);
    }

    public void SpawnCreature(PlayerManager player, CardInfo card){
        _kingdom.RpcReplaceRecruitTile(card.title, GetNewCreatureFromDb());

        var scriptableCard = Resources.Load<ScriptableCard>("Cards/CreatureCards/" + card.title);
        var cardObject = Instantiate(creatureCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Discard);
    }
    
    private CardInfo SpawnAndCacheCard(PlayerManager owner, GameObject cardObject, ScriptableCard scriptableCard){
        var instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        Cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);

        var cardInfo = new CardInfo(scriptableCard, instanceID);
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);

        return cardInfo;
    }

    private void MoveSpawnedCard(PlayerManager owner, GameObject cardObject,
                                 CardInfo cardInfo, CardLocation destination){
        switch (destination)
        {
            case CardLocation.Deck:
                owner.deck.Add(cardInfo);
                break;
            case CardLocation.Discard:
                owner.discard.Add(cardInfo);
                break;
            case CardLocation.Hand:
                owner.hand.Add(cardInfo);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destination), destination, null);
        }

        if(cardSpawnAnimations) owner.RpcSpawnCard(cardObject, destination);
        else owner.RpcMoveCard(cardObject, CardLocation.Spawned, destination);
    }

    public void SpawnFieldEntity(PlayerManager owner, GameObject card, CardType type)
    {
        GameObject entityObject;
        if(type == CardType.Creature) entityObject = Instantiate(creatureEntityPrefab);
        else entityObject = Instantiate(developmentEntityPrefab);
        
        NetworkServer.Spawn(entityObject, connectionToClient);
        entityObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);

        var entity = entityObject.GetComponent<BattleZoneEntity>();
        var opponent = GetOpponent(owner);
        _boardManager.AddEntity(owner, opponent, card, entity);
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

    public void SetNumberOfKingdomTiles(int money, int development, int creature)
    {
        _nbMoneyTiles = money;
        _nbDevelopTiles = development;
        _nbCreatureTiles = creature;
    }

    private IEnumerator PreGameWaitRoutine(){
        yield return new WaitForSeconds(6f);
        foreach(var player in players.Keys) {
            player.deck.Shuffle();
            player.DrawInitialHand(initialHandSize);
        }
        OnGameStart?.Invoke(players.Count);
    }

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
