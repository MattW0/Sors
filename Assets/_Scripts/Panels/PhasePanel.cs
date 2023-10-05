using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasePanel : NetworkBehaviour
{
    public static PhasePanel Instance { get; private set; }
    [SerializeField] private List<Phase> _selectedPhases = new();
    [SerializeField] private PhaseItemUI attack;
    [SerializeField] private PhaseItemUI block;

    [Header("UI Elements")]
    [SerializeField] private GameObject confirmPhaseSelection;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text actionDescriptionText;
    
    private bool _animate;
    private int _nbPhasesToChose;
    private PhasePanelUI _phasePanelUI;
    private TurnScreenOverlay _turnScreenOverlay;
    public static event Action OnPhaseSelectionStarted;
    public static event Action OnPhaseSelectionConfirmed;
    
    private void Awake() {
        if (!Instance) Instance = this;

        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
    }

    #region Prepare and Phase Selection
    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPlayers, int nbPhasesToChose, bool animations){
        _phasePanelUI = PhasePanelUI.Instance;
        _phasePanelUI.PrepareUI(nbPlayers);
        _turnScreenOverlay = TurnScreenOverlay.Instance;

        _nbPhasesToChose = nbPhasesToChose;
        _animate = animations;
    }

    [ClientRpc]
    public void RpcBeginPhaseSelection(int currentTurn){
        turnText.text = "Turn " + currentTurn.ToString();
        OnPhaseSelectionStarted?.Invoke();

        if(!_animate) return;
        _turnScreenOverlay.UpdateTurnScreen(currentTurn);
    }

    public void UpdateSelectedPhase(Phase phase){
        if (_selectedPhases.Contains(phase)){
            _selectedPhases.Remove(phase);
        } else {
            _selectedPhases.Add(phase);
        }
        
        if (_selectedPhases.Count == _nbPhasesToChose){
            // confirmPhaseSelection.SetActive(true);
            ConfirmButtonPressed();
        }
    }

    public void ConfirmButtonPressed(){
        actionDescriptionText.text = "Wait for opponent...";
        confirmPhaseSelection.SetActive(false);

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }
    #endregion

    #region Phases

    [ClientRpc]
    private void RpcUpdatePhaseHighlight(TurnState newState) {
        var newHighlightIndex = newState switch
        {
            TurnState.PhaseSelection => 0,
            TurnState.Draw => 1,
            TurnState.Invent => 2,
            TurnState.Develop => 3,
            TurnState.Recruit => 6,
            TurnState.Deploy => 7,
            TurnState.Prevail => 8,
            TurnState.CleanUp => 9,
            _ => -1
        };

        _phasePanelUI.UpdatePhaseHighlight(newHighlightIndex);
    }
    
    [ClientRpc]
    private void RpcUpdateCombatHighlight(CombatState newState) {
        var newHighlightIndex = newState switch
        {
            CombatState.Attackers => 4,
            CombatState.Blockers => 5,
            _ => -1
        };
        
        _phasePanelUI.UpdatePhaseHighlight(newHighlightIndex);
    }

    #endregion

    #region Combat

    [ClientRpc]
    public void RpcStartCombatPhase(CombatState state){
        if (state == CombatState.Attackers) BeginCombatAttack();
        else if (state == CombatState.Blockers) BeginCombatBlock();
    }
    
    private void BeginCombatAttack(){
        actionDescriptionText.text = "Select attackers";
        attack.StartCombatPhase();
    }
    private void BeginCombatBlock(){
        actionDescriptionText.text = "Select blockers";
        block.StartCombatPhase();
    }

    [TargetRpc]
    public void TargetDisableCombatButtons(NetworkConnection conn){
        attack.Reset();
        block.Reset();
    }

    public void PlayerPressedCombatButton(){
        var player = PlayerManager.GetLocalPlayer();
        player.PlayerPressedCombatButton();
    }

    #endregion

    [ClientRpc]
    public void RpcChangeActionDescriptionText(TurnState state){
        var text = state switch {
            TurnState.PhaseSelection => "Select " + _nbPhasesToChose.ToString() + " phases",
            TurnState.Discard => "Discard cards",
            TurnState.Invent => "Buy developments",
            TurnState.Develop => "Play developments",
            TurnState.Recruit => "Buy creatures",
            TurnState.Deploy => "Play creatures",
            TurnState.Prevail => "Choose prevail options",
            TurnState.CardIntoHand => "Put a card in your hand",
            TurnState.Trash => "Trash cards",
            _ => ""
        };

        actionDescriptionText.text = text;
    }

    private void OnDestroy() {
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
    }
}