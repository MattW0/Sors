using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public bool debug = false;
    public bool animations = false;

    public static GameManager Instance { get; private set; }
    private TurnManager _turnManager;
    private Kingdom _kingdom;
    public List<PlayerManager> players;

    [Header("Game state")]
    [SyncVar] public int turnNb = 0;

    [Header("Turn specifics")]
    public int nbPhasesToChose = 2;
    public int nbCardDraw = 2;
    public int nbDiscard = 1;
    public int turnCash = 0;
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
        PlayerSetup();

        _turnManager.UpdateTurnState(TurnState.Prepare);
    }

    private void KingdomSetup(){

        GameObject kingdomObject = Instantiate(_kingdomPrefab, transform);
        NetworkServer.Spawn(kingdomObject, connectionToClient);
        _kingdom = Kingdom.Instance;

        CardInfo[] kingdomCards = new CardInfo[nbKingdomCards];
        
        for (int i = 0; i < nbKingdomCards; i++)
        {
            ScriptableCard card = creatureCards[Random.Range(0, creatureCards.Length)];
            kingdomCards[i] = new CardInfo(card);
        }

        _kingdom.RpcSetKingdomCards(kingdomCards);
    }

    private void PlayerSetup(){
        players.Clear();
        players.AddRange(FindObjectsOfType<PlayerManager>());

        foreach (var player in players)
        {   
            player.RpcFindObjects(debug);

            // UI
            player.RpcSetPlayerStats(startHealth, startScore);
            player.Cash = turnCash;
            player.Recruits = turnRecruits;

            // Cards
            SpawnPlayerDeck(player);
            player.cards.deck.Shuffle();
        }

        PlayersDrawInitialHands();
    }

    private void SpawnPlayerDeck(PlayerManager player){
        // Coppers
        for (int i = 0; i < initialDeckSize - nbCreatures; i++){
            ScriptableCard card = moneyCards[0];
            GameObject cardObject = Instantiate(_moneyCardPrefab);
            SpawnCacheAndMoveCard(cardObject, card, player, CardLocations.Deck);
        }

        // Other start cards
        for (int i = 0; i < nbCreatures; i++){
            ScriptableCard card = startCards[i];
            GameObject cardObject = Instantiate(_cardPrefab);
            SpawnCacheAndMoveCard(cardObject, card, player, CardLocations.Deck);
        }
    }

    private void SpawnCacheAndMoveCard(GameObject cardObject, ScriptableCard scriptableCard, 
                                       PlayerManager player, CardLocations destination){

        string instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        Cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);

        var cardInfo = new CardInfo(scriptableCard, instanceID);        
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);
        switch (destination)
        {
            case CardLocations.Deck:
                player.cards.deck.Add(cardInfo);
                break;
            case CardLocations.Discard:
                player.cards.discard.Add(cardInfo);
                break;
            case CardLocations.Hand:
                player.cards.hand.Add(cardInfo);
                break;
        }
        player.RpcMoveCard(cardObject, CardLocations.Spawned, destination);
    }

    private void PlayersDrawInitialHands(){
        foreach (var player in players) {
            player.DrawCards(initialHandSize);
            player.RpcCardPilesChanged();
        }
    }

    public void SpawnCreature(PlayerManager player, CardInfo cardInfo){
        // print("GameManager: SpawnCreature");

        var scriptableCard = Resources.Load<ScriptableCard>("CreatureCards/" + cardInfo.title);
        var cardObject = Instantiate(_cardPrefab);
        SpawnCacheAndMoveCard(cardObject, scriptableCard, player, CardLocations.Discard);
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
