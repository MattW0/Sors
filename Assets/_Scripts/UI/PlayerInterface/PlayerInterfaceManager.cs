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
            TurnState.Draw => 0,
            TurnState.Invent => 1,
            TurnState.Develop => 2,
            TurnState.Recruit => 5,
            TurnState.Deploy => 6,
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

    [ClientRpc]
    public void RpcLog(string message, LogType type){
        
        var messageColor = type switch{
            LogType.EffectTrigger => ColorManager.effectTrigger,
            LogType.TurnChange => ColorManager.turnChange,
            LogType.Phase => ColorManager.phase,
            LogType.CreatureBuy => ColorManager.creatureBuy,
            LogType.Combat => ColorManager.combat,
            LogType.CombatDamage => ColorManager.combatDamage,
            LogType.CombatClash => ColorManager.combatClash,
            LogType.Standard => ColorManager.standardLog,
            // _ => ColorManager.standardLog
        };

        _logger.Log($"<color={messageColor}>{message}</color>");
    } 

    private void OnDestroy()
    {
        GameManager.OnGameStart -= RpcPrepareUIs;
        TurnManager.OnPhasesSelected -= RpcShowPlayerChoices;
        TurnManager.OnPhaseChanged -= RpcUpdatePhaseHighlight;
        CombatManager.OnCombatStateChanged -= RpcUpdateCombatHighlight;
    }
}

public enum LogType{
    Standard,
    EffectTrigger,
    TurnChange,
    Phase,
    CreatureBuy,
    Combat,
    CombatDamage,
    CombatClash
}
