using Mirror;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private PhaseVisualsUI phaseVisualsUI;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        TurnManager.OnGameStarting += RpcPrepareUIs;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
    }

    [ClientRpc]
    private void RpcPrepareUIs()
    {
        phaseVisualsUI = PhaseVisualsUI.Instance;
        phaseVisualsUI.PrepareUI();
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

        phaseVisualsUI.UpdatePhaseHighlight(newHighlightIndex);
    }
    
    [ClientRpc]
    private void RpcUpdateCombatHighlight(CombatState newState) {
        var newHighlightIndex = newState switch
        {
            CombatState.Attackers => 3,
            CombatState.Blockers => 4,
            _ => -1
        };
        
        phaseVisualsUI.UpdatePhaseHighlight(newHighlightIndex);
    }


    public void OnResignButtonPressed() {
        var player = GameManager.GetPlayerManager();
        print(player.name);
    }
    
    public void OnSkipButtonPressed() {
        var player = GameManager.GetPlayerManager();
    }
    
    public void OnReadyButtonPressed() {
        var player = GameManager.GetPlayerManager();
    }

    private void OnDestroy()
    {
        TurnManager.OnGameStarting -= RpcPrepareUIs;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
    }
}
