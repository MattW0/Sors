using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager instance;
    private GameManager gameManager;
    
    [Header("Turn state")]
    [SyncVar][SerializeField] private TurnState state;
    public List<Phase> chosenPhases = new List<Phase>();
    public static event Action<TurnState> OnTurnStateChanged;
    private static int turnCount = 0;
    private static int playersReady = 0;

    [Header("Objects")]
    public GameObject phasePanelPrefab;
    [SerializeField] private GameObject phasePanel;

    void Awake() {
        if (instance == null) instance = this;
    }

    void Start() {
        UpdateTurnState(TurnState.Init);

        gameManager = GameManager.instance;
    }

    public void UpdateTurnState(TurnState newState){
        state = newState;

        switch(state){
            case TurnState.Init:
                break;
            case TurnState.PhaseSelection:
                PhaseSelection();
                break;
            case TurnState.DrawI:
                DrawI();
                break;
            case TurnState.Recruit:
                break;
            case TurnState.Attack:
                break;
            case TurnState.Combat:
                break;
            case TurnState.DrawII:
                break;
            case TurnState.BuyCard:
                break;
            case TurnState.Develop:
                break;
            case TurnState.CleanUp:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnTurnStateChanged?.Invoke(state);
    }

    // public void Ready(){
    //     playersReady++;
    //     if(playersReady == gameManager.players.Count){
    //         UpdateTurnState(TurnState.DrawI);
    //     }
    // }

    private void PhaseSelection() {
        phasePanel = Instantiate(phasePanelPrefab, transform);
        NetworkServer.Spawn(phasePanel, connectionToClient);

        playersReady = 0;
    }

    public void PlayerSelectedPhases(List<Phase> phases) {
        
        playersReady++;
        foreach (Phase phase in phases) {
            if(!chosenPhases.Contains(phase)){
                chosenPhases.Add(phase);
            }
        }
        
        if (playersReady == gameManager.players.Count) BeginPhases();
    }

    private void BeginPhases(){
        
        NetworkServer.Destroy(phasePanel);
        playersReady = 0;

        chosenPhases.Sort();

        // Going through phases chosen by players
        while(chosenPhases.Count > 0){
            Enum.TryParse(chosenPhases[0].ToString(), out TurnState nextState);
            chosenPhases.RemoveAt(0);
            UpdateTurnState(nextState);
        }
    }

    private void DrawI() {

        //TODO: Draw and discard

        foreach (PlayerManager player in gameManager.players) {
            
            int _nbCardDraw = gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawI)) _nbCardDraw++;

            for (int i = 0; i < _nbCardDraw; i++) {
                player.DrawCard();
            }
        }
        
        foreach (PlayerManager player in gameManager.players) player.DiscardCards(gameManager.nbDiscard);
    }
}

public enum TurnState
{
    Init,
    WaitingForReady,
    PhaseSelection,
    DrawI,
    Recruit,
    Attack,
    Combat,
    DrawII,
    BuyCard,
    Develop,
    CleanUp
}

public enum Phase
{
    DrawI,
    Recruit,
    Attack,
    DrawII,
    BuyCard,
    Develop,
}
