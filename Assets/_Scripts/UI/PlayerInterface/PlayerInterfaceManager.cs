using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    private PlayerInterfaceButtons _buttons;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += RpcPrepareUIs;
    }

    [ClientRpc]
    private void RpcPrepareUIs(int nbPlayers){
        _buttons = PlayerInterfaceButtons.Instance;
    }

    [ClientRpc]
    public void RpcLog(string message, LogType type){
        
        var messageColor = GetColor(type);
        _logger.Log($"<color={messageColor}>{message}</color>");
    }

    // public static void Log(string message, LogType type){
    //     var messageColor = GetColor(type);
    //     _logger.Log($"<color={messageColor}>{message}</color>");
    // }


    // static does not work
    private string GetColor(LogType type){
        var messageColor = type switch{
            LogType.EffectTrigger => SorsColors.effectTrigger,
            LogType.TurnChange => SorsColors.turnChange,
            LogType.Phase => SorsColors.phase,
            LogType.CreatureBuy => SorsColors.creatureBuy,
            LogType.Combat => SorsColors.combat,
            LogType.CombatDamage => SorsColors.combatDamage,
            LogType.CombatClash => SorsColors.combatClash,
            LogType.Standard => SorsColors.standardLog,
            _ => SorsColors.standardLog
        };

        return messageColor;
    }

    private void OnDestroy(){
        GameManager.OnGameStart -= RpcPrepareUIs;
    }
}

public enum LogType : byte
{
    Standard,
    EffectTrigger,
    TurnChange,
    Phase,
    CreatureBuy,
    Combat,
    CombatDamage,
    CombatClash
}
