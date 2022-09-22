using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public bool debug = false;
    public bool animations = false;

    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    private Kingdom _kingdom;
    public Dictionary<PlayerManager, NetworkIdentity> players;

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
    [SerializeField] private int nbKingdomCards = 16;
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
    [SerializeField] private GameObject _kingdomPrefab;
    [SerializeField] private GameObject _discardPanelPrefab;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private GameObject _moneyCardPrefab;

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
        _turnManager = TurnManager.Instance;

        KingdomSetup();
        PanelSetup();
        PlayerSetup();

        _turnManager.UpdateTurnState(TurnState.Prepare);
    }

    private void KingdomSetup(){
        var kingdomObject = Instantiate(_kingdomPrefab, transform);
        NetworkServer.Spawn(kingdomObject, connectionToClient);
        _kingdom = Kingdom.Instance;

        var kingdomCards = new CardInfo[nbKingdomCards];
        
        for (var i = 0; i < nbKingdomCards; i++)
        {
            var card = creatureCards[Random.Range(0, creatureCards.Length)];
            kingdomCards[i] = new CardInfo(card);
        }

        _kingdom.RpcSetKingdomCards(kingdomCards);
    }

    private void PanelSetup()
    {
        var discardPanelObject = Instantiate(_discardPanelPrefab, transform);
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
            player.Cash = turnCash;
            player.Recruits = turnRecruits;

            // Cards
            SpawnPlayerDeck(player, playerNetworkId);
            player.cards.deck.Shuffle();
        }

        PlayersDrawInitialHands();
    }

    private void SpawnPlayerDeck(PlayerManager playerManager, NetworkIdentity playerNetworkId){
        // Coppers
        for (var i = 0; i < initialDeckSize - nbCreatures; i++){
            var moneyCard = moneyCards[0]; // Only copper right now
            var cardObject = Instantiate(_moneyCardPrefab);
            SpawnCacheAndMoveCard(playerManager, playerNetworkId, cardObject, moneyCard, CardLocations.Deck);
        }

        // Other start cards
        for (var i = 0; i < nbCreatures; i++){
            var creatureCard = startCards[i]; // Special creatures 'A' & 'B' 
            var cardObject = Instantiate(_cardPrefab);
            SpawnCacheAndMoveCard(playerManager, playerNetworkId, cardObject, creatureCard, CardLocations.Deck);
        }
    }

    private void SpawnCacheAndMoveCard(PlayerManager playerManager, NetworkIdentity playerNetworkId,
                                       GameObject cardObject, ScriptableCard scriptableCard, CardLocations destination){

        string instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        Cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(playerNetworkId.connectionToClient);

        var cardInfo = new CardInfo(scriptableCard, instanceID);        
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);
        switch (destination)
        {
            case CardLocations.Deck:
                playerManager.cards.deck.Add(cardInfo);
                break;
            case CardLocations.Discard:
                playerManager.cards.discard.Add(cardInfo);
                break;
            case CardLocations.Hand:
                playerManager.cards.hand.Add(cardInfo);
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
        playerManager.RpcMoveCard(cardObject, CardLocations.Spawned, destination);
    }

    private void PlayersDrawInitialHands(){
        foreach (var player in players.Keys) {
            player.DrawCards(initialHandSize);
            player.RpcCardPilesChanged();
        }
    }

    public void SpawnCreature(PlayerManager player, NetworkIdentity playerNetworkId, CardInfo cardInfo){
        var scriptableCard = Resources.Load<ScriptableCard>("CreatureCards/" + cardInfo.title);
        var cardObject = Instantiate(_cardPrefab);
        SpawnCacheAndMoveCard(player, playerNetworkId, cardObject, scriptableCard, CardLocations.Discard);
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
