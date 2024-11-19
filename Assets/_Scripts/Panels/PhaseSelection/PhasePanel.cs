using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PhasePanel : NetworkBehaviour
{
    [SerializeField] private List<TurnState> _selectedPhases = new();
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
        _phasePanelUI = GetComponent<PhasePanelUI>();

        TurnManager.OnStartPhaseSelection += RpcStartSelection;
        TurnManager.OnTurnStateChanged += RpcUpdatePhaseHighlight;

        CombatManager.OnCombatStateChanged += RpcUpdatePhaseHighlight;
        CombatPhaseItemUI.OnPressedCombatButton += PlayerPressedCombatButton;

        PhaseItemUI.OnToggleSelection += UpdateSelectedPhase;
    }

    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhases)
    {
        _nbPhasesToChose = nbPhases;
        _localPlayer = PlayerManager.GetLocalPlayer();
        _combatManager = CombatManager.Instance;
    }

    [ClientRpc]
    private void RpcStartSelection() => OnPhaseSelectionStarted?.Invoke();
    
    [ClientRpc]
    public void RpcShowPhaseSelection(PlayerManager player, TurnState[] phases)
    {
        _phasePanelUI.HighlightPhasesToPlay(phases);

        if (player.isLocalPlayer) return;
        _phasePanelUI.ShowOpponentChoices(phases);
    }

    #region Phases

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

    private void UpdateSelectedPhase(TurnState phase)
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

    private void ConfirmButtonPressed()
    {
        // actionDescriptionText.text = "Wait for opponent...";
        _localPlayer.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }

    private void OnDestroy() 
    {
        TurnManager.OnStartPhaseSelection -= RpcStartSelection;
        TurnManager.OnTurnStateChanged -= RpcUpdatePhaseHighlight;

        CombatManager.OnCombatStateChanged -= RpcUpdatePhaseHighlight;
        CombatPhaseItemUI.OnPressedCombatButton -= PlayerPressedCombatButton;
        
        PhaseItemUI.OnToggleSelection -= UpdateSelectedPhase;
    }
}