using Mirror;
using UnityEngine;

public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }

    private TurnManager _turnManager;
    private PhaseVisualsUI _phaseVisualsUI;
    private PlayerInterfaceButtons _buttons;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += RpcPrepareUIs;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
        BoardManager.OnSkipCombatPhase += PlayerDisableReadyButton;
        
        _turnManager = TurnManager.Instance;
    }

    [ClientRpc]
    private void RpcPrepareUIs()
    {
        _phaseVisualsUI = PhaseVisualsUI.Instance;
        _buttons = PlayerInterfaceButtons.Instance;
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

    private void PlayerDisableReadyButton(PlayerManager player){
        TargetDisableReadyButon(player.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [TargetRpc]
    private void TargetDisableReadyButon(NetworkConnection target){
        // Needs to be target rpc
        _buttons.DisableReadyButton();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= RpcPrepareUIs;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
        BoardManager.OnSkipCombatPhase -= PlayerDisableReadyButton;
    }
}
