using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    private TurnManager _turnManager;
    [SerializeField] private Logger _logger;
    private PhaseVisuals _phaseVisualsUI;
    private PlayerInterfaceButtons _buttons;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += RpcPrepareUIs;
        TurnManager.OnPhasesSelected += RpcShowPlayerChoices;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
        
        _turnManager = TurnManager.Instance;
    }

    [ClientRpc]
    private void RpcPrepareUIs(int nbPlayers)
    {
        _phaseVisualsUI = PhaseVisuals.Instance;
        _buttons = PlayerInterfaceButtons.Instance;
     
        _phaseVisualsUI.PrepareUI(nbPlayers);
    }

    [ClientRpc]
    private void RpcShowPlayerChoices(Phase[] choices){
        _phaseVisualsUI.ShowPlayerChoices(choices);
    }

    [ClientRpc]
    private void RpcUpdatePhaseHighlight(TurnState newState) {
        var newHighlightIndex = newState switch
        {
            TurnState.DrawI => 0,
            TurnState.Develop => 1,
            TurnState.Deploy => 2,
            TurnState.DrawII => 5,
            TurnState.Recruit => 6,
            TurnState.Prevail => 7,
            TurnState.CleanUp => -2,
            _ => -1
        };

        _phaseVisualsUI.UpdatePhaseHighlight(newHighlightIndex);
        _buttons.EnableReadyButton();
    }
    
    [ClientRpc]
    private void RpcUpdateCombatHighlight(CombatState newState) {
        var newHighlightIndex = newState switch
        {
            CombatState.Attackers => 3,
            CombatState.Blockers => 4,
            _ => -1
        };
        
        _phaseVisualsUI.UpdatePhaseHighlight(newHighlightIndex);
        _buttons.EnableReadyButton();
    }

    [TargetRpc]
    public void TargetDisableReadyButton(NetworkConnection target){
        _buttons.DisableReadyButton();
    }

    [ClientRpc]
    public void RpcLog(string message) => _logger.Log(message);

    private void OnDestroy()
    {
        GameManager.OnGameStart -= RpcPrepareUIs;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
    }
}
