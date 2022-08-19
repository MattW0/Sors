using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }
    private GameManager gameManager;
    
    [Header("Turn state")]
    [SerializeField] private TurnState state;
    public List<Phase> chosenPhases = new List<Phase>();
    public static event Action<TurnState> OnTurnStateChanged;
    private static int playersReady = 0;

    [Header("Objects")]
    [SerializeField] private GameObject _phasePanelPrefab;
    private GameObject _phasePanel;
    [SerializeField] private GameObject _discardPanelPrefab;
    private GameObject _discardPanel;

    // public static event Action OnCardPileNumberChanged;


    void Awake() {
        if (Instance == null) Instance = this;
    }

    void Start() {
        gameManager = GameManager.Instance;
        UpdateTurnState(TurnState.Prepare);
    }

    public void UpdateTurnState(TurnState _newState){
        state = _newState;

        if (_newState != TurnState.NextPhase) {
            print($"<color=aqua>Turn changed to {_newState}</color>");
        }

        switch(state){
            // --- Preparation and transition ---
            case TurnState.Prepare:
                Prepare();
                break;
            case TurnState.PhaseSelection:
                PhaseSelection();
                break;
            case TurnState.NextPhase:
                NextPhase();
                break;
            // --- Phases ---
            case TurnState.DrawI:
                DrawI();
                break;
            case TurnState.Discard:
                Discard();
                break;
            case TurnState.Recruit:
                Recruit();
                break;
            case TurnState.Attack:
                Attack();
                break;
            case TurnState.Combat:
                Combat();
                break;
            case TurnState.DrawII:
                DrawI();
                break;
            case TurnState.BuyCard:
                BuyCard();
                break;
            case TurnState.Develop:
                Develop();
                break;
            // --- Win check and turn reset ---
            case TurnState.CleanUp:
                CleanUp();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_newState), _newState, null);
        }

        OnTurnStateChanged?.Invoke(state);
    }

    private void Prepare() {
        print("<color=yellow>Prepare not yet implemented</color>");

        // UpdateTurnState(TurnState.PhaseSelection);
    }
 
    private void PhaseSelection() {

        gameManager.turnNb++;

        _phasePanel = Instantiate(_phasePanelPrefab, transform);
        NetworkServer.Spawn(_phasePanel, connectionToClient);

        playersReady = 0;
    }

    public void PlayerSelectedPhases(List<Phase> phases) {
        
        playersReady++;
        foreach (Phase phase in phases) {
            if(!chosenPhases.Contains(phase)){
                chosenPhases.Add(phase);
            }
        }
        
        if (!(playersReady == gameManager.players.Count)) return;

        // Starting the selected phases
        NetworkServer.Destroy(_phasePanel);
        chosenPhases.Sort();
        print($"<color=white>Chosen Phases: {string.Join(", ", chosenPhases)}</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase(){
        
        if (chosenPhases.Count == 0) {
            UpdateTurnState(TurnState.CleanUp);
            return;
        }

        Enum.TryParse(chosenPhases[0].ToString(), out TurnState nextPhase);
        chosenPhases.RemoveAt(0);
        UpdateTurnState(nextPhase);
    }

    private void DrawI() {

        foreach (PlayerManager player in gameManager.players) {
            
            int _nbCardDraw = gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawI)) _nbCardDraw++;

            player.DrawCards(_nbCardDraw);
        }

        UpdateTurnState(TurnState.Discard);
    }

    private void Discard() {

        playersReady = 0;
        _discardPanel = Instantiate(_discardPanelPrefab, transform);
        NetworkServer.Spawn(_discardPanel, connectionToClient);

        // foreach (PlayerManager player in gameManager.players) {
        //     NetworkIdentity nwIdentity = player.gameObject.GetComponent<NetworkIdentity>();
        //     player.TargetDiscardCards(nwIdentity.connectionToClient, gameManager.nbDiscard);
        // }
    }

    public void PlayerSelectedDiscardCards(){
        playersReady++;
        if (!(playersReady == gameManager.players.Count)) return ;

        foreach (PlayerManager player in gameManager.players) {
            player.RpcDiscardSelection();
        }

        NetworkServer.Destroy(_discardPanel);
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Recruit(){
        print("<color=yellow>Recruit not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Attack(){
        print("<color=yellow>Attack not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Combat(){
        print("<color=yellow>Combat not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void DrawII(){
        print("<color=yellow>DrawII not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void BuyCard(){
        print("<color=yellow>BuyCard not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Develop(){
        print("<color=yellow>Develop not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void CleanUp(){

        print("<color=yellow>CleanUp not yet implemented</color>");
        UpdateTurnState(TurnState.PhaseSelection);
    }

}

public enum TurnState
{
    Prepare,
    NextPhase,
    WaitingForReady,
    PhaseSelection,
    DrawI,
    Discard,
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
