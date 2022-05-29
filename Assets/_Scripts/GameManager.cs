using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public bool debug = false;
    public static GameManager instance;
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

    public void Awake()
    {
        if (instance == null) instance = this;

        startCards = Resources.LoadAll<ScriptableCard>("StartCards/");
        creatureCards = Resources.LoadAll<ScriptableCard>("CreatureCards/");
        moneyCards = Resources.LoadAll<ScriptableCard>("MoneyCards/");
    }

    public void GameSetup()
    {
        turnManager = TurnManager.instance;

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
        turnManager.UpdateTurnState(TurnState.PhaseSelection);
    }

    private void SpawnPlayerDeck(PlayerManager player){
        // Coppers
        for (int i = 0; i < initialDeckSize - nbCreatures; i++){
            ScriptableCard card = moneyCards[0];
            player.SpawnCard(card);
        }
        // Creatures (special)
        for (int i = 0; i < nbCreatures; i++){
            // ScriptableCard card = creatureCards[Random.Range(0, creatureCards.Length)];
            ScriptableCard card = startCards[i];
            player.SpawnCard(card);
        }
    }

    private void PlayersDrawInitialHands(){
        foreach (PlayerManager player in players) {
            for (int i = 0; i < initialHandSize; i++) {
                player.DrawCard();
            }
        }
    }
}
