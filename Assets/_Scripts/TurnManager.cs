using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager instance;
    public List<PlayerManager> players = new List<PlayerManager>();
    
    [Header("Turn state")]
    [SyncVar][SerializeField] private TurnState state;
    public static event Action<TurnState> OnTurnStateChanged;
    private static int turnCount = 0;
    private static int playersReady = 0;

    [Header("Objects")]
    public GameObject phasePanelPrefab;
    [SerializeField] private GameObject phasePanel;
    PhasePanelUI phasePanelUI;

    void Awake() {
        if (instance == null) instance = this;
    }

    void Start() {
        UpdateTurnState(TurnState.Idle);
    }

    public void UpdateTurnState(TurnState newState){
        state = newState;

        switch(state){
            case TurnState.Idle:
                break;
            case TurnState.PhaseSelection:
                PhaseSelection();
                break;
            case TurnState.DrawI:
                Debug.Log("DrawI");
                break;
            case TurnState.Recruit:
                break;
            case TurnState.Attack:
                break;
            case TurnState.Combat:
                break;
            case TurnState.DrawII:
                break;
            case TurnState.Buy:
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

    public void Ready(){
        playersReady++;
        if(playersReady == players.Count){
            UpdateTurnState(TurnState.DrawI);
        }
    }

    private void PhaseSelection() {
        phasePanel = Instantiate(phasePanelPrefab, transform);
        NetworkServer.Spawn(phasePanel, connectionToClient);

        playersReady = 0;
        PhasePanelUI.onSelectionConfirmend += SelectedPhases;
    }

    private void SelectedPhases() {
        playersReady++;
        print("playersReady: " + playersReady);
        if (playersReady == players.Count) {
            UpdateTurnState(TurnState.DrawI);
        }
    }
}

public enum TurnState
{
    Idle,
    PhaseSelection,
    DrawI,
    Recruit,
    Attack,
    Combat,
    DrawII,
    Buy,
    Develop,
    CleanUp
}
