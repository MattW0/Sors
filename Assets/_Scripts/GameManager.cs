using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public bool debug = false;
    public static GameManager Instance { get; private set; }
    private TurnManager turnManager;
    public List<PlayerManager> players = new List<PlayerManager>();

    [Header("Game state")]
    public int turnNb = 0;

    [Header("Turn specifics")]
    public int nbCardDraw = 2;
    public int nbDiscard = 1;

    [Header("Game start settings")]
    public int initialDeckSize = 10;
    public int nbCreatures = 2;
    public int initialHandSize = 4;
    public int startHealth = 30;
    public int startScore = 0;

    [Header("Available cards")]
    public ScriptableCard[] startCards;
    public ScriptableCard[] creatureCards;
    public ScriptableCard[] moneyCards;
    [SerializeField] private GameObject _cardPrefab;

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

        // Player setup
        players.Clear();
        players.AddRange(FindObjectsOfType<PlayerManager>());

        foreach (PlayerManager player in players)
        {   
            player.RpcSetUI(startHealth, startScore);
            SpawnPlayerDeck(player);
            player.cards.deck.Shuffle();
        }

        PlayersDrawInitialHands();

        turnNb = 1;
        turnManager.UpdateTurnState(TurnState.PhaseSelection);
    }

    private void SpawnPlayerDeck(PlayerManager player){
        // Coppers
        for (int i = 0; i < initialDeckSize - nbCreatures; i++){
            ScriptableCard card = moneyCards[0];
            SpawnCard(card, player);
        }
        // Creatures (special)
        for (int i = 0; i < nbCreatures; i++){
            // ScriptableCard card = creatureCards[Random.Range(0, creatureCards.Length)];
            ScriptableCard card = startCards[i];
            SpawnCard(card, player);
        }
    }

    private void SpawnCard(ScriptableCard card, PlayerManager player){

        GameObject cardObject = Instantiate(_cardPrefab);
        string instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        NetworkServer.Spawn(cardObject, connectionToClient);
        cardObject.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);

        CardInfo cardInfo = new CardInfo(card, instanceID);
        player.cards.deck.Add(cardInfo);
        cardObject.GetComponent<CardUI>().RpcSetCardUI(cardInfo);

        player.RpcMoveCard(cardObject, "DrawPile");
    }

    private void PlayersDrawInitialHands(){
        foreach (PlayerManager player in players) {
            for (int i = 0; i < initialHandSize; i++) {
                player.DrawCard();
            }
        }
    }
}
