using Mirror;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }

    private TurnManager _turnManager;
    private PhaseVisualsUI _phaseVisualsUI;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        TurnManager.OnGameStarting += RpcPrepareUIs;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
        
        _turnManager = TurnManager.Instance;
    }

    [ClientRpc]
    private void RpcPrepareUIs()
    {
        _phaseVisualsUI = PhaseVisualsUI.Instance;
        _phaseVisualsUI.PrepareUI();
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
    }


    public void OnResignButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
    }
    
    public void OnUndoButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
    }
    
    public void OnReadyButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
        player.PlayerPressedReadyButton();
    }

    private void OnDestroy()
    {
        TurnManager.OnGameStarting -= RpcPrepareUIs;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
    }
}
