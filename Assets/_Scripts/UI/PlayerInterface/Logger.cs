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

        Log($"This is a test message", "Origin", lineType);
    }

    public void Log(string message, string originator, LogType type)
    {
        // Get timestamp for message
        var time = System.DateTime.Now.ToString("HH:mm:ss");
        Instantiate(_textMessagePrefab, _logTransform).SetMessage(
            new Message(originator, time, SorsColors.AddColorByType(message, type))
        );
    }

    internal void ToggleVisible() => gameObject.SetActive(!gameObject.activeSelf);
}

public enum LogType : byte
{
    Standard,
    EffectTrigger,
    AbilityExecution,
    TurnChange,
    Phase,
    Buy,
    Play,
    Combat,
    CombatAttacker,
    CombatBlocker,
    CombatClash,
    AbilityTarget
}