using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    
    public LogType lineType;
    public bool printLine;

    void Update()
    {
        if(!printLine) return;
        printLine = false;

        Log($"This is test message", lineType);
    }

    public void Log(string message, LogType type){
        var messageColor = GetColor(type);
        logText.text += $"<color={messageColor}>{message}</color>\n";
        scrollRect.ScrollToBottom();
    }

    private string GetColor(LogType type){
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