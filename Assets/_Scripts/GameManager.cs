using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Mirror;
using Random = UnityEngine.Random;
using GameState;

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
    private int _nbTechnologyTiles;
    private int _nbCreatureTiles;
    [SerializeField] private int initialDeckSize = 10;
    [SerializeField] private int initialCreatures = 3;
    [SerializeField] private int initialTechnologies = 2;
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
    public ScriptableCard[] startTechnologies;
    public ScriptableCard[] creatureCardsDb;
    public ScriptableCard[] moneyCardsDb;
    public ScriptableCard[] technologyCardsDb;
    private List<int> _availableCreatureIds = new();
    private List<int> _availableTechnologyIds = new();
    private Sprite[] _moneySprites;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject technologyCardPrefab;
    [SerializeField] private GameObject creatureEntityPrefab;
    [SerializeField] private GameObject technologyEntityPrefab;

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

        TurnManager.OnPlayerDies += PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
        
        LoadCards();
    }

    private void LoadCards(){
        _moneySprites = Resources.LoadAll<Sprite>("Sprites/Money/");
        startCreatures = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/Creatures/");
        startTechnologies = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/Technologies/");

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

        // Start game from state -> skip normal setup and start from there
        if(! string.IsNullOrWhiteSpace(options.StateFile)) {
            LoadGameState(options.StateFile);
            return;
        }

        KingdomSetup();
        PlayerSetup();

        if(cardSpawnAnimations) StartCoroutine(PreGameWaitRoutine());
    }

    #region Setup
    private void LoadGameState(string fileName){

        print($"Loading game state from file: {fileName}");
        
        var stateFile = Resources.Load<TextAsset>("GameStates/" + fileName);
        var state = JsonUtility.FromJson<GameState.GameState>(stateFile.text);

        KingdomSetup();
        PlayerSetupFromFile(state.players);
    }

    private void PlayerSetupFromFile(Player[] playerData)
    {
        Player host = new Player();
        Player client = new Player();

        foreach (var p in playerData){
            if (p.isHost) host = p;
            else client = p;
        }

        var playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (var player in playerManagers)
        {
            var playerNetworkId = player.GetComponent<NetworkIdentity>();
            players.Add(player, playerNetworkId);
            player.RpcInitPlayer();

            // p has player info and game state 
            Player p = new Player();
            if(player.isLocalPlayer) p = host;
            else p = client;

            // To update string in network
            player.PlayerName = p.playerName;
            SpawnCardsFromFile(player, p.cards);
            SpawnEntitiesFromFile(player, p.entities);
        }

        OnGameStart?.Invoke(players.Count);
    }

    private void SpawnCardsFromFile(PlayerManager p, Cards cards)
    {
        foreach(var c in cards.handCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            SpawnCardAndMoveTo(p, scriptableCard, CardLocation.Hand);
        }

        foreach(var c in cards.deckCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            SpawnCardAndMoveTo(p, scriptableCard, CardLocation.Deck);
        }

        foreach(var c in cards.discardCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            SpawnCardAndMoveTo(p, scriptableCard, CardLocation.Discard);
        }
    }

    private void SpawnEntitiesFromFile(PlayerManager p, Entities entities)
    {
        foreach(var e in entities.creatures){
            var scriptableCard = Resources.Load<ScriptableCard>(e);
            var cardObject = SpawnCardAndMoveTo(p, scriptableCard, CardLocation.PlayZone);
            StartCoroutine(WaitForCardInit(p, cardObject, scriptableCard.type));
        }

        foreach(var e in entities.technologies){
            var scriptableCard = Resources.Load<ScriptableCard>(e);
            var cardObject = SpawnCardAndMoveTo(p, scriptableCard, CardLocation.PlayZone);
            StartCoroutine(WaitForCardInit(p, cardObject, scriptableCard.type));
        }
    }

    private IEnumerator WaitForCardInit(PlayerManager p, GameObject card, CardType type){
        yield return new WaitForSeconds(0.1f);
        SpawnFieldEntity(p, card, type, false);
        yield return null;
    }
    
    private void KingdomSetup(){
        _kingdom.RpcSetPlayer();
        
        // Money: right now only paper money
        var moneyCards = new CardInfo[_nbMoneyTiles];
        for (var i = 0; i < _nbMoneyTiles; i++) moneyCards[i] = new CardInfo(moneyCardsDb[i]);
        _kingdom.RpcSetMoneyTiles(moneyCards);

        var technologies = new CardInfo[_nbTechnologyTiles];
        for (var i = 0; i < _nbTechnologyTiles; i++) technologies[i] = GetNewTechnologyFromDb();
        _kingdom.RpcSetTechnologyTiles(technologies);

        // Recruit Creatures
        var creatures = new CardInfo[_nbCreatureTiles];
        for (var i = 0; i < _nbCreatureTiles; i++) creatures[i] = GetNewCreatureFromDb();
        _kingdom.RpcSetRecruitTiles(creatures);
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

    private void SpawnPlayerDeck(PlayerManager playerManager)
    {
        // Only paper money currently
        for (var i = 0; i < initialDeckSize - initialCreatures - initialTechnologies; i++){
            var scriptableCard = moneyCardsDb[0];
            SpawnCardAndMoveTo(playerManager, scriptableCard, CardLocation.Deck);
        }

        for (var i = 0; i < initialCreatures; i++){
            var scriptableCard = startCreatures[i]; // 
            SpawnCardAndMoveTo(playerManager, scriptableCard, CardLocation.Deck);
        }

        for (var i = 0; i < initialTechnologies; i++){
            var scriptableCard = startTechnologies[i];
            SpawnCardAndMoveTo(playerManager, scriptableCard, CardLocation.Deck);
        }
    }

    #endregion

    #region Spawning
    private GameObject SpawnCardAndMoveTo(PlayerManager player, ScriptableCard card, CardLocation destination){
        
        // Card object prefab depending on type
        var cardObject = card.type switch
        {
            CardType.Money => Instantiate(moneyCardPrefab) as GameObject,
            CardType.Creature => Instantiate(creatureCardPrefab) as GameObject,
            CardType.Technology => Instantiate(technologyCardPrefab) as GameObject,
            CardType.None => null,
            _ => null
        };

        // Assign client authority and put in cache
        var cardInfo = SpawnAndCacheCard(player, cardObject, card);

        // Add card to player collection list and move 
        MoveSpawnedCard(player, cardObject, cardInfo, destination);

        return cardObject;
    }

    public void PlayerGainMoney(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/MoneyCards/" + card.hash + "_" + card.title);
        SpawnCardAndMoveTo(player, scriptableCard, CardLocation.Discard);
    }

    public void PlayerGainTechnology(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/TechnologyCards/" + card.title);
        SpawnCardAndMoveTo(player, scriptableCard, CardLocation.Discard);
    }

    public void PlayerGainCreature(PlayerManager player, CardInfo card){
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/CreatureCards/" + card.title);
        SpawnCardAndMoveTo(player, scriptableCard, CardLocation.Discard);        
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
            case CardLocation.PlayZone:
                // When loading game state
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destination), destination, null);
        }

        if(cardSpawnAnimations) owner.RpcSpawnCard(cardObject, destination);
        else owner.RpcMoveCard(cardObject, CardLocation.Spawned, destination);
    }

    public void SpawnFieldEntity(PlayerManager owner, GameObject card, CardType type, bool isPlayed = true)
    {
        // Entity object prefab depending on type
        GameObject entityObject = type switch
        {
            CardType.Creature => Instantiate(creatureEntityPrefab) as GameObject,
            CardType.Technology => Instantiate(technologyEntityPrefab) as GameObject,
            _ => null
        };
        
        NetworkServer.Spawn(entityObject, connectionToClient);
        entityObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);

        var entity = entityObject.GetComponent<BattleZoneEntity>();
        var opponent = GetOpponent(owner);

        _boardManager.AddEntity(owner, opponent, card, entity, isPlayed);
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

    #region Utils
    private CardInfo GetNewTechnologyFromDb()
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

    public void SetNumberOfKingdomTiles(int moneyTiles, int technologies, int creatures)
    {
        _nbMoneyTiles = moneyTiles;
        _nbTechnologyTiles = technologies;
        _nbCreatureTiles = creatures;
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
    #endregion

    private void OnDestroy()
    {
        TurnManager.OnPlayerDies -= PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
    }
}
