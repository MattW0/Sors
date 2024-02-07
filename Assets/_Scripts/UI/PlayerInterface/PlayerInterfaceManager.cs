using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    private PlayerInterfaceButtons _buttons;

    public LogType lineType;
    public bool printLine;
    
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += RpcPrepareUIs;
    }

    void Update()
    {
        if(!printLine) return;
        printLine = false;

        var messageColor = GetColor(lineType);
        _logger.Log($"<color={messageColor}>This is test message</color>");
    }

    [ClientRpc]
    private void RpcPrepareUIs(GameOptions gameOptions){
        _buttons = PlayerInterfaceButtons.Instance;
    }

    [ClientRpc]
    public void RpcLog(string message, LogType type){
        
        var messageColor = GetColor(type);
        _logger.Log($"<color={messageColor}>{message}</color>");
    }

    private string GetColor(LogType type){
        var messageColor = type switch{
            LogType.EffectTrigger => SorsColors.effectTrigger,
            LogType.TurnChange => SorsColors.turnChange,
            LogType.Phase => SorsColors.detail,
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
