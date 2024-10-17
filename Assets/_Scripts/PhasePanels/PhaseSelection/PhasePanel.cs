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
    public static event Action<int> OnPhaseSelectionStarted;
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

    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhases)
    {
        _nbPhasesToChose = nbPhases;
        _localPlayer = PlayerManager.GetLocalPlayer();
        _combatManager = CombatManager.Instance;
    }

    
    [ClientRpc]
    public void RpcShowPhaseSelection(PlayerManager player, Phase[] phases)
    {
        for (int i = 0; i < phases.Length; i++) _phasePanelUI.IsPlayedThisTurn(phases[i]);

        if (player.isLocalPlayer) return;
        _phasePanelUI.ShowOpponentChoices(phases);
    }

    #region Phases

    [ClientRpc]
    private void RpcBeginPhaseSelection(int turnNumber) => OnPhaseSelectionStarted?.Invoke(turnNumber);

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

    private void ConfirmButtonPressed()
    {
        // actionDescriptionText.text = "Wait for opponent...";
        _localPlayer.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }

    private void OnDestroy() 
    {
        TurnManager.OnBeginTurn -= RpcBeginPhaseSelection;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdatePhaseHighlight;

        PhaseItemUI.OnToggleSelection -= UpdateSelectedPhase;
        CombatPhaseItemUI.OnPressedCombatButton -= PlayerPressedCombatButton;
    }
}