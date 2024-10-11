using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PhasePanel : NetworkBehaviour
{
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

    #endregion

    #region Combat

    [ClientRpc]
    public void RpcStartCombatPhase(TurnState state){
        // actionDescriptionText.text = $"Select {state}";

        if (state == TurnState.Attackers) attack.IsSelectable();
        else if (state == TurnState.Blockers) block.IsSelectable();
    }

    [TargetRpc]
    public void TargetDisableCombatButtons(NetworkConnection conn)
    {
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