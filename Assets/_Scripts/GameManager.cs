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
    [SyncVar] public int turnNumber;
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

    [Header("Helpers")]
    [SerializeField] private Transform _entitySpawnTransform;

    // Caching all gameObjects of cards in game
    private static Dictionary<int, GameObject> Cache { get; set; } = new();
    public static GameObject GetCardObject(int goID) { return Cache[goID]; }
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

    #region Setup
    
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
        }

        StartCoroutine(PreGameWaitRoutine());
    }

    private void SpawnPlayerDeck(PlayerManager player)
    {
        List<GameObject> startCards = new();
        // Only paper money currently
        for (var i = 0; i < initialDeckSize - initialCreatures - initialTechnologies; i++){
            var scriptableCard = moneyCardsDb[0];
            startCards.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        for (var i = 0; i < initialCreatures; i++){
            var scriptableCard = startCreatures[i]; // 
            startCards.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        for (var i = 0; i < initialTechnologies; i++){
            var scriptableCard = startTechnologies[i];
            startCards.Add(SpawnCardAndAddToCollection(player, scriptableCard, CardLocation.Deck));
        }

        player.RpcShowSpawnedCards(startCards, CardLocation.Deck);
    }

    #endregion

    #region Game State Loading

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

        Dictionary<PlayerManager, Cards> playerCards = new();
        Dictionary<PlayerManager, Entities> playerEntities = new();
        foreach (var player in FindObjectsOfType<PlayerManager>())
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

            playerCards.Add(player, p.cards);
            playerEntities.Add(player, p.entities);
        }

        StartCoroutine(SpawningFromFile(playerCards, playerEntities));
    }

    private IEnumerator SpawningFromFile(Dictionary<PlayerManager, Cards> playerCards, Dictionary<PlayerManager, Entities> playerEntities)
    {
        foreach(var (player, cards) in playerCards){
            StartCoroutine(SpawnCardsFromFile(player, cards));

            yield return new WaitForSeconds(6f);

            StartCoroutine(SpawnEntitiesFromFile(player, playerEntities[player]));
        }

        yield return new WaitForSeconds(2f);
        
        OnGameStart?.Invoke(players.Count);
    }

    private IEnumerator SpawnCardsFromFile(PlayerManager p, Cards cards)
    {
        List<GameObject> cardList = new();
        foreach(var c in cards.handCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Hand));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Hand);
        cardList.Clear();

        yield return new WaitForSeconds(2f);

        foreach(var c in cards.deckCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Deck));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Deck);
        cardList.Clear();

        yield return new WaitForSeconds(2f);

        foreach(var c in cards.discardCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Discard));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Discard);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator SpawnEntitiesFromFile(PlayerManager p, Entities entities)
    {
        var entitiesDict = new Dictionary<GameObject, BattleZoneEntity>();

        foreach(var e in entities.creatures){
            var scriptableCard = Resources.Load<ScriptableCard>(e);
            var cardObject = SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            yield return new WaitForSeconds(0.1f);
            var entity = SpawnFieldEntity(p, cardObject);

            entitiesDict.Add(cardObject, entity);
        }

        foreach(var e in entities.technologies){
            var scriptableCard = Resources.Load<ScriptableCard>(e);
            var cardObject = SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            yield return new WaitForSeconds(0.1f);
            var entity = SpawnFieldEntity(p, cardObject);

            entitiesDict.Add(cardObject, entity);
        }

        p.RpcShowSpawnedCards(entitiesDict.Keys.ToList(), CardLocation.PlayZone);
        _boardManager.PlayEntities(entitiesDict, fromFile: true);
    }

    #endregion

    #region Spawning
    private GameObject SpawnCardAndAddToCollection(PlayerManager player, ScriptableCard scriptableCard, CardLocation destination)
    {
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

    public void PlayerGainMoney(PlayerManager player, CardInfo cardInfo)
    {
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/MoneyCards/" + cardInfo.hash + "_" + cardInfo.title);
        ResolveCardGain(player, scriptableCard);
    }

    public void PlayerGainTechnology(PlayerManager player, CardInfo card)
    {
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/TechnologyCards/" + card.title);
        ResolveCardGain(player, scriptableCard);
    }

    public void PlayerGainCreature(PlayerManager player, CardInfo card)
    {
        var scriptableCard = Resources.Load<ScriptableCard>("Cards/CreatureCards/" + card.title);
        ResolveCardGain(player, scriptableCard);
    }

    private void ResolveCardGain(PlayerManager player, ScriptableCard card)
    {
        var cardObject = SpawnCardAndAddToCollection(player, card, CardLocation.Discard);
        player.RpcShowSpawnedCard(cardObject, CardLocation.Discard);
    }
    
    private int SpawnAndCacheCard(PlayerManager owner, GameObject cardObject, ScriptableCard scriptableCard){
        // Using the unique gameObject instance ID ()
        var instanceID = cardObject.GetInstanceID();
        cardObject.name = instanceID.ToString();
        Cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);

        return instanceID;
    }

    private CardInfo IntitializeCardOnClients(GameObject cardObject, ScriptableCard scriptableCard, int instanceID){
        // Client RPC : Init card UI and disable gameObject (cause not yet on UI layer)
        var cardInfo = new CardInfo(scriptableCard, instanceID);
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);

        return cardInfo;
    }

    private void AddCardToPlayerCollection(PlayerManager owner, CardInfo cardInfo, CardLocation destination){
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
    }

    public BattleZoneEntity SpawnFieldEntity(PlayerManager owner, GameObject card)
    {
        var cardInfo = card.GetComponent<CardStats>().cardInfo;

        // Entity prefab depending on type
        GameObject entityObject = cardInfo.type switch
        {
            CardType.Creature => Instantiate(creatureEntityPrefab, _entitySpawnTransform) as GameObject,
            CardType.Technology => Instantiate(technologyEntityPrefab, _entitySpawnTransform) as GameObject,
            _ => null
        };
        
        // Assign authority
        NetworkServer.Spawn(entityObject, connectionToClient);
        entityObject.GetComponent<NetworkIdentity>().AssignClientAuthority(players[owner].connectionToClient);
        var entity = entityObject.GetComponent<BattleZoneEntity>();

        // Intitialize entity on clients
        entity.RpcInitializeEntity(owner, GetOpponent(owner), cardInfo);
        
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

        // TODO: Need anohter approach of timing concept
        // Use async / await ?
        // How does this work with rpc calls and animations executing on players?

        yield return new WaitForSeconds(6f);

        foreach(var player in players.Keys) {
            player.deck.Shuffle();
            player.DrawInitialHand(initialHandSize);
        }

        yield return new WaitForSeconds(1f);
        OnGameStart?.Invoke(players.Count);
    }

    public PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Keys.FirstOrDefault(p => p != player);
    }
    #endregion

    private void OnDestroy()
    {
        TurnManager.OnPlayerDies -= PlayerDies;
        SorsNetworkManager.OnAllPlayersReady -= GameSetup;
    }
}
