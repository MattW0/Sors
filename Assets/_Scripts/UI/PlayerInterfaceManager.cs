using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }

    private TurnManager _turnManager;
    private PlayerInterfacePhaseVisuals _phaseVisualsUI;
    private PlayerInterfaceButtons _buttons;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += RpcPrepareUIs;
        TurnManager.OnPhasesSelected += RpcShowPlayerChoices;
        TurnManager.OnPhaseChanged += RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged += RpcUpdateCombatHighlight;
        BoardManager.OnSkipCombatPhase += DisableReadyButtonForPlayer;
        
        _turnManager = TurnManager.Instance;
    }

    [ClientRpc]
    private void RpcPrepareUIs()
    {
        _phaseVisualsUI = PlayerInterfacePhaseVisuals.Instance;
        _buttons = PlayerInterfaceButtons.Instance;
        _phaseVisualsUI.PrepareUI(GameManager.Instance.players.Count);
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

    [Server]
    private void DisableReadyButtonForPlayer(PlayerManager player){

        var target = player.GetComponent<NetworkIdentity>().connectionToClient;
        TargetDisableReadyButton(target);
    }

    [TargetRpc]
    private void TargetDisableReadyButton(NetworkConnection target){
        _buttons.DisableReadyButton();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= RpcPrepareUIs;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
        BoardManager.OnSkipCombatPhase -= DisableReadyButtonForPlayer;
    }
}
