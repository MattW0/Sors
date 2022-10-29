using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [Header("For Coding")]
    public bool debug = false;
    public bool animations = false;

    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    public Dictionary<PlayerManager, NetworkIdentity> players;
    public static event Action OnGameStart;
    public static event Action<PlayerManager, BattleZoneEntity, GameObject> OnEntitySpawned; 

    [Header("Game state")]
    [SyncVar] public int turnNb = 0;

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
    public ScriptableCard[] creatureCards;
    public ScriptableCard[] moneyCards;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject discardPanelPrefab;
    [SerializeField] private GameObject creatureCardPrefab;
    [SerializeField] private GameObject moneyCardPrefab;
    [SerializeField] private GameObject entityObjectPrefab;

    // Caching all gameObjects of cards in game
    private Dictionary<string, GameObject> Cache { get; set; }

    public GameObject GetCardObject(string goID) { return Cache[goID]; }

    public void Awake()
    {
        if (Instance == null) Instance = this;

        startCards = Resources.LoadAll<ScriptableCard>("StartCards/");
        creatureCards = Resources.LoadAll<ScriptableCard>("CreatureCards/");
        moneyCards = Resources.LoadAll<ScriptableCard>("MoneyCards/");

        Cache = new Dictionary<string, GameObject>();
    }

    public void GameSetup()
    {
        // _turnManager = TurnManager.Instance;
        
        KingdomSetup();
        PanelSetup();
        PlayerSetup();
        
        OnGameStart?.Invoke();
    }

    private void KingdomSetup(){
        var kingdomCards = new CardInfo[nbKingdomCards];
        
        for (var i = 0; i < nbKingdomCards; i++)
        {
            var card = creatureCards[Random.Range(0, creatureCards.Length)];
            kingdomCards[i] = new CardInfo(card);
        }

        Kingdom.Instance.RpcSetKingdomCards(kingdomCards);
    }

    private void PanelSetup()
    {
        var discardPanelObject = Instantiate(discardPanelPrefab, transform);
        NetworkServer.Spawn(discardPanelObject, connectionToClient);
        
        discardPanelObject.gameObject.SetActive(false);
    }

    private void PlayerSetup(){
        players = new Dictionary<PlayerManager, NetworkIdentity>();
        var playerManagers = FindObjectsOfType<PlayerManager>();

        foreach (var player in playerManagers)
        {
            var playerNetworkId = player.GetComponent<NetworkIdentity>();
            players.Add(player, playerNetworkId);
            
            player.RpcFindObjects(debug);

            // UI
            player.RpcSetPlayerStats(startHealth, startScore);
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

    private void SpawnPlayerDeck(PlayerManager playerManager){
        // Coppers
        for (var i = 0; i < initialDeckSize - nbCreatures; i++){
            var moneyCard = moneyCards[0]; // Only copper right now
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
        var scriptableCard = Resources.Load<ScriptableCard>("CreatureCards/" + cardInfo.title);
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

    private PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Keys.FirstOrDefault(p => p != player);
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
