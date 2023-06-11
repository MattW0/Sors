using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    [SerializeField] private int nbRecruitTiles = 8;
    [SerializeField] private int nbMoneyTiles = 4;
    [SerializeField] private int nbDevelopTiles = 8;
    [SerializeField] public int initialDeckSize = 10;
    [SerializeField] public int initialCreatures = 3;
    [SerializeField] public int initialHandSize = 4;
    [SerializeField] public int startHealth = 30;
    [SerializeField] public int startScore = 0;

    [Header("Turn specifics")]
    public int nbPhasesToChose;
    [SerializeField] public int nbCardDraw = 2;
    [SerializeField] public int nbDiscard = 1;
    public int turnCash = 0;
    public int turnInvents = 1; 
    public int turnDevelops = 1; 
    public int turnDeploys = 1; 
    public int turnRecruits = 1;
    public int prevailOptionsToChoose = 1;

    [Header("Turn Boni")]
    public int inventPriceReduction = 1;
    public int developPriceReduction = 1;
    public int recruitPriceReduction = 1;
    public int prevailExtraOptions = 1;
    public int deployBonusDeploys = 1;


    [Header("Available cards")]
    public ScriptableCard[] startCreatures;
    public ScriptableCard[] creatureCardsDb;
    public ScriptableCard[] moneyCardsDb;
    public ScriptableCard[] developCardsDb;
    private List<int> _cardsIdCache = new();
    private Sprite[] _moneySprites;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject developCardPrefab;
    [SerializeField] private GameObject entityObjectPrefab;

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

        startCreatures = Resources.LoadAll<ScriptableCard>("CreatureCards/StartCreatures/");

        creatureCardsDb = Resources.LoadAll<ScriptableCard>("CreatureCards/");
        moneyCardsDb = Resources.LoadAll<ScriptableCard>("MoneyCards/");
        developCardsDb = Resources.LoadAll<ScriptableCard>("DevelopCards/");

        print("Creature cards: " + creatureCardsDb.Length);
        print("Money cards: " + moneyCardsDb.Length);
        print("Develop cards: " + developCardsDb.Length);

        _moneySprites = Resources.LoadAll<Sprite>("Sprites/Money/");

        TurnManager.OnPlayerDies += PlayerDies;
        SorsNetworkManager.OnAllPlayersReady += GameSetup;
    }

    public void GameSetup(int nbPlayers, int nbPhases, bool fullHand, bool spawnimations){

        print("Game starting with options:");
        print("Players: " + nbPlayers);
        print("Phases: " + nbPhases);
        print("Full hand: " + fullHand);
        print("Spawnimations: " + spawnimations);

        singlePlayer = nbPlayers == 1;
        nbPhasesToChose = nbPhases;
        initialHandSize = fullHand ? initialDeckSize : initialHandSize;
        cardSpawnAnimations = spawnimations;

        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _kingdom = Kingdom.Instance;
        _endScreen = EndScreen.Instance;
        
        KingdomSetup();
        PlayerSetup();

        if(cardSpawnAnimations) StartCoroutine(PreGameWaitRoutine());
    }

    #region Setup
    
    private void KingdomSetup(){
        // Developments: right now only money
        var moneyCards = new CardInfo[nbMoneyTiles];
        moneyCards[0] = new CardInfo(moneyCardsDb[1]); // Silver
        moneyCards[1] = new CardInfo(moneyCardsDb[2]); // Gold
        moneyCards[2] = new CardInfo(moneyCardsDb[0]); // Gold
        moneyCards[3] = new CardInfo(moneyCardsDb[0]); // Gold
        _kingdom.RpcSetMoneyTiles(moneyCards);

        var developCards = new CardInfo[nbDevelopTiles];
        for (var i = 0; i < nbDevelopTiles; i++) developCards[i] = GetNewDevelopmentFromDb();
        _kingdom.RpcSetDevelopmentTiles(developCards);

        // Recruits: Creatures
        var recruitCards = new CardInfo[nbRecruitTiles];
        for (var i = 0; i < nbRecruitTiles; i++) recruitCards[i] = GetNewCreatureFromDb();
        _kingdom.RpcSetRecruitTiles(recruitCards);
    }

    private CardInfo GetNewDevelopmentFromDb(){
        // Get new random card
        // var id = Random.Range(0, developmentCardsDb.Length);
        // while (_cardsIdCache.Contains(id)) id = Random.Range(0, creatureCardsDb.Length);

        // _cardsIdCache.Add(id);
        // var card = creatureCardsDb[id];

        // return new CardInfo(card);

        return new CardInfo(developCardsDb[0]);
    }

    private CardInfo GetNewCreatureFromDb(){
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
                OnGameStart?.Invoke(players.Count);
            } else {
                player.RpcResolveCardSpawn(cardSpawnAnimations);
            }
        }
    }

    #endregion

    #region Spawning
    
    private void SpawnPlayerDeck(PlayerManager playerManager){
        // Coppers
        for (var i = 0; i < initialDeckSize - initialCreatures; i++){
            var moneyCard = moneyCardsDb[0]; // Only copper right now
            var cardObject = Instantiate(moneyCardPrefab) as GameObject;
            var cardInfo = SpawnAndCacheCard(playerManager, cardObject, moneyCard);
            MoveSpawnedCard(playerManager, cardObject, cardInfo, CardLocation.Deck);
        }

        // Other start cards
        for (var i = 0; i < initialCreatures; i++){
            var creatureCard = startCreatures[i]; // Special creatures 'Deathy', 'Firsty', 'Trampy'
            var cardObject = Instantiate(creatureCardPrefab) as GameObject;
            var cardInfo = SpawnAndCacheCard(playerManager, cardObject, creatureCard);
            MoveSpawnedCard(playerManager, cardObject, cardInfo, CardLocation.Deck);
        }
    }

    public void SpawnMoney(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("MoneyCards/" + card.hash + "_" + card.title);
        var cardObject = Instantiate(moneyCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Discard);
    }

    public void SpawnDevelopment(PlayerManager player, CardInfo card){
        // using hash as index for currency scriptable objects
        var scriptableCard = Resources.Load<ScriptableCard>("DevelopCards/" + card.title);
        var cardObject = Instantiate(developCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Discard);
    }

    public void SpawnCreature(PlayerManager player, CardInfo card){
        _kingdom.RpcReplaceRecruitTile(card.title, GetNewCreatureFromDb());

        var scriptableCard = Resources.Load<ScriptableCard>("creatureCards/" + card.title);
        var cardObject = Instantiate(creatureCardPrefab);
        var cardInfo = SpawnAndCacheCard(player, cardObject, scriptableCard);
        MoveSpawnedCard(player, cardObject, cardInfo, CardLocation.Deck);
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

    public void SpawnFieldEntity(PlayerManager owner, GameObject card)
    {
        var entityObject = Instantiate(entityObjectPrefab);
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
