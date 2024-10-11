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
    [SerializeField] private CombatPhaseItemUI attack;
    [SerializeField] private CombatPhaseItemUI block;
    
    private int _nbPhasesToChose;
    private PhasePanelUI _phasePanelUI;
    private PlayerManager _localPlayer;
    private CombatManager _combatManager;
    public static event Action OnPhaseSelectionStarted;
    public static event Action OnPhaseSelectionConfirmed;
    
    private void Awake() 
    {
        if (!Instance) Instance = this;

        TurnManager.OnBeginTurn += RpcBeginPhaseSelection;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdatePhaseHighlight;

        PhaseItemUI.OnToggleSelection += UpdateSelectedPhase;
        CombatPhaseItemUI.OnPressedCombatButton += PlayerPressedCombatButton;

        _phasePanelUI = GetComponent<PhasePanelUI>();
    }

    #region Prepare and Phase Selection

    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhases)
    {
        // print("Prepare phase panel: " + nbPhases);

        _nbPhasesToChose = nbPhases;
        _localPlayer = PlayerManager.GetLocalPlayer();
        _combatManager = CombatManager.Instance;
    }

    private void ConfirmButtonPressed()
    {
        // actionDescriptionText.text = "Wait for opponent...";
        _localPlayer.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }
    #endregion

    #region Phases

    [ClientRpc]
    private void RpcBeginPhaseSelection(int turnNumber)
    {
        // turnText.text = "Turn " + turnNumber.ToString();
        OnPhaseSelectionStarted?.Invoke();

        // _turnScreenOverlay.UpdateTurnScreen(turnNumber);
    }

    private void UpdateSelectedPhase(Phase phase)
    {
        if (_selectedPhases.Contains(phase)){
            _selectedPhases.Remove(phase);
        } else {
            _selectedPhases.Add(phase);
        }
        
        if (_selectedPhases.Count == _nbPhasesToChose){
            ConfirmButtonPressed();
        }
    }

    [ClientRpc]
    public void RpcShowOpponentChoices(PlayerManager player, Phase[] phases)
    {
        if (player.isLocalPlayer) return;
        _phasePanelUI.ShowOpponentChoices(phases);
    }

    [ClientRpc]
    private void RpcUpdatePhaseHighlight(TurnState newState) => _phasePanelUI.UpdatePhaseHighlight(newState);
    
    // [ClientRpc]
    // private void RpcCombatStateChanged(TurnState newState) 
    // {
    //     var newHighlightIndex = newState switch
    //     {
    //         TurnState.Attackers => 4,
    //         CombatState.Blockers => 5,
    //         _ => -1
    //     };

    //     _phasePanelUI.UpdatePhaseHighlight(newHighlightIndex);
    // }

    #endregion

    #region Combat

    [ClientRpc]
    public void RpcStartCombatPhase(TurnState state){
        if (state == TurnState.Attackers) BeginCombatAttack();
        else if (state == TurnState.Blockers) BeginCombatBlock();
    }
    
    private void BeginCombatAttack(){
        // actionDescriptionText.text = "Select attackers";
        attack.StartCombatPhase();
    }
    private void BeginCombatBlock(){
        // actionDescriptionText.text = "Select blockers";
        block.StartCombatPhase();
    }

    [TargetRpc]
    public void TargetDisableCombatButtons(NetworkConnection conn){
        attack.Reset();
        block.Reset();
    }

    private void PlayerPressedCombatButton() => CmdPlayerPressedCombatButton(PlayerManager.GetLocalPlayer());
    [Command(requiresAuthority = false)]
    private void CmdPlayerPressedCombatButton(PlayerManager player) => _combatManager.PlayerPressedReadyButton(player);

    #endregion

    private void OnDestroy() 
    {
        TurnManager.OnBeginTurn -= RpcBeginPhaseSelection;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdatePhaseHighlight;

        PhaseItemUI.OnToggleSelection -= UpdateSelectedPhase;
        CombatPhaseItemUI.OnPressedCombatButton -= PlayerPressedCombatButton;
    }
}