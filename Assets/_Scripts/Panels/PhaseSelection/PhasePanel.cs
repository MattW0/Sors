using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(PhasePanelUI))]
public class PhasePanel : NetworkBehaviour
{
    [SerializeField] private List<TurnState> _selectedPhases = new();
    private int _nbPhasesToChose;
    private PhasePanelUI _phasePanelUI;
    private PlayerManager _localPlayer;
    [SerializeField] private InteractionUI _interactionPanelUI;
    public static event Action OnPhaseSelectionConfirmed;
    public static event Action OnReset;
    
    private void Awake() 
    {
        _phasePanelUI = GetComponent<PhasePanelUI>();

        OptionalPhaseItemUI.OnToggleSelection += UpdateSelectedPhase;
        TurnManager.OnTurnStateChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdatePhaseHighlight;
        InteractionPanel.OnConfirmPhaseSelection += PlayerConfirms;
        InteractionPanel.OnResetPhaseSelection += PlayerResets;
    }

    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhases)
    {
        _nbPhasesToChose = nbPhases;
        _localPlayer = PlayerManager.GetLocalPlayer();
    }

    [ClientRpc]
    public void RpcShowPhaseSelection(PlayerManager player, List<TurnState> phases)
    {
        _phasePanelUI.HighlightPhasesToPlay(phases);

        if (player.isLocalPlayer) return;
        _phasePanelUI.ShowOpponentChoices(phases);
    }

    #region Phases

    [ClientRpc]
    private void RpcUpdatePhaseHighlight(TurnState newState) => _phasePanelUI.UpdatePhaseHighlight(newState);

    #endregion

    private void UpdateSelectedPhase(TurnState phase)
    {
        if (_selectedPhases.Contains(phase)){
            _selectedPhases.Remove(phase);
        } else {
            _selectedPhases.Add(phase);
        }

        _interactionPanelUI.SetConfirmButtonEnabled(_selectedPhases.Count == _nbPhasesToChose);
    }

    private void PlayerConfirms()
    {
        _localPlayer.CmdPhaseSelection(_selectedPhases);
        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }

    private void PlayerResets()
    {
        _interactionPanelUI.SetConfirmButtonEnabled(false);
        _selectedPhases.Clear();
        OnReset?.Invoke();
    }

    private void OnDestroy() 
    {
        OptionalPhaseItemUI.OnToggleSelection -= UpdateSelectedPhase;
        TurnManager.OnTurnStateChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdatePhaseHighlight;
        InteractionPanel.OnConfirmPhaseSelection -= PlayerConfirms;
        InteractionPanel.OnResetPhaseSelection -= PlayerResets;
    }
}