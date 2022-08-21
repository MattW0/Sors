using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public bool debug = false;
    public bool animations = false;

    public static GameManager Instance { get; private set; }
    private TurnManager turnManager;
    private Kingdom _kingdom;
    public List<PlayerManager> players = new List<PlayerManager>();

    [Header("Game state")]
    [SyncVar] public int turnNb = 0;

    [Header("Turn specifics")]
    public int nbPhasesToChose = 2;
    public int nbCardDraw = 2;
    public int nbDiscard = 1;
    public int turnCash = 0;
    public int turnRecruits = 1;

    [Header("Game start settings")]
    [SerializeField] private int _nbKingomCards = 16;
    public int initialDeckSize = 10;
    public int nbCreatures = 2;
    public int initialHandSize = 4;
    public int startHealth = 30;
    public int startScore = 0;

    [Header("Available cards")]
    [SerializeField] private GameObject _kingdomPrefab;
    public ScriptableCard[] startCards;
    public ScriptableCard[] creatureCards;
    public ScriptableCard[] moneyCards;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private GameObject _moneyCardPrefab;

    // Caching all gameObjects of cards in game
    private Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> Cache{ get => _cache; }
    public GameObject GetCardObject(string _goID) { return Cache[_goID]; }

    public void Awake()
    {
        if (Instance == null) Instance = this;

        startCards = Resources.LoadAll<ScriptableCard>("StartCards/");
        creatureCards = Resources.LoadAll<ScriptableCard>("CreatureCards/");
        moneyCards = Resources.LoadAll<ScriptableCard>("MoneyCards/");
    }

    public void GameSetup()
    {
        turnManager = TurnManager.Instance;

        KingdomSetup();
        PlayerSetup();

        turnManager.UpdateTurnState(TurnState.Prepare);
    }

    private void KingdomSetup(){

        GameObject _kingdomObject = Instantiate(_kingdomPrefab, transform);
        NetworkServer.Spawn(_kingdomObject, connectionToClient);
        _kingdom = Kingdom.Instance;

        CardInfo[] _kingdomCards = new CardInfo[_nbKingomCards];
        
        for (int i = 0; i < _nbKingomCards; i++)
        {
            ScriptableCard _card = creatureCards[Random.Range(0, creatureCards.Length)];
            _kingdomCards[i] = new CardInfo(_card);
        }

        _kingdom.RpcSetKingdomCards(_kingdomCards);
    }

    private void PlayerSetup(){
        players.Clear();
        players.AddRange(FindObjectsOfType<PlayerManager>());

        foreach (PlayerManager player in players)
        {   
            player.RpcFindOpponent(debug);

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
            SpawnCacheAndMoveCard(cardObject, card, player, "DrawPile");

        }

        // Other start cards
        for (int i = 0; i < nbCreatures; i++){
            ScriptableCard card = startCards[i];
            GameObject cardObject = Instantiate(_cardPrefab);
            SpawnCacheAndMoveCard(cardObject, card, player, "DrawPile");
        }
    }

    private void SpawnCacheAndMoveCard(GameObject cardObject, ScriptableCard scriptableCard, 
                                       PlayerManager player, string destination){

        string instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        _cache.Add(instanceID, cardObject);

        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);

        CardInfo cardInfo = new CardInfo(scriptableCard, instanceID);        
        cardObject.GetComponent<CardStats>().RpcSetCardStats(cardInfo);
        player.cards.deck.Add(cardInfo);
        player.RpcMoveCard(cardObject, destination);
    }

    private void PlayersDrawInitialHands(){
        foreach (PlayerManager player in players) {
            player.DrawCards(initialHandSize);
            player.RpcCardPilesChanged();
        }
    }

    public void SpawnCreature(PlayerManager player, CardInfo cardInfo){
        print("GameManager: SpawnCreature");

        ScriptableCard scriptableCard = Resources.Load<ScriptableCard>("CreatureCards/" + cardInfo.title);
        GameObject cardObject = Instantiate(_cardPrefab);
        SpawnCacheAndMoveCard(cardObject, scriptableCard, player, "DiscardPile");
    }
}
