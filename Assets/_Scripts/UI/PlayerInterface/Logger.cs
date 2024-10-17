using System;
using Mirror.Examples.Basic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [SerializeField] private TextMessage _textMessagePrefab;
    [SerializeField] private Transform _logTransform;
    
    public LogType lineType;
    public bool printLine;

    void Update()
    {
        if(!printLine) return;
        printLine = false;

        Log($"This is a test message", lineType);
    }

    public void Log(string message, LogType type)
    {
        // Get timestamp for message
        var time = System.DateTime.Now.ToString("HH:mm:ss");
        
        // TODO: How to define originator and get player color?
        var originator = "<color=#AAAAAA> ORIGINATOR </color>";
        var text = $"<color={GetColor(type)}>{message}</color>";

        Instantiate(_textMessagePrefab, _logTransform).SetMessage(new Message(originator, time, text));
    }

    public void LogWithOriginator(string message, int originatorId, LogType type)
    {
        // Get timestamp for message
        var time = System.DateTime.Now.ToString("HH:mm:ss");
        var text = $"<color={GetColor(type)}>{message}</color>";

        Instantiate(_textMessagePrefab, _logTransform).SetMessage(new Message(originatorId.ToString(), time, text));
    }

    private string GetColor(LogType type)
    {
        var messageColor = type switch{
            LogType.EffectTrigger => SorsColors.effectTrigger,
            LogType.TurnChange => SorsColors.turnChange,
            LogType.Phase => SorsColors.detail,
            LogType.Buy => SorsColors.buy,
            LogType.Play => SorsColors.play,
            LogType.Combat => SorsColors.combat,
            LogType.CombatAttacker => SorsColors.combatAttacker,
            LogType.CombatBlocker => SorsColors.combatBlocker,
            LogType.CombatClash => SorsColors.ColorToHex(SorsColors.combatClash),
            LogType.Standard => SorsColors.standardLog,
            _ => SorsColors.standardLog
        };

        return messageColor;
    }

    internal void ToggleVisible() => gameObject.SetActive(!gameObject.activeSelf);
}

public enum LogType : byte
{
    Standard,
    EffectTrigger,
    TurnChange,
    Phase,
    Buy,
    Play,
    Combat,
    CombatAttacker,
    CombatBlocker,
    CombatClash
}