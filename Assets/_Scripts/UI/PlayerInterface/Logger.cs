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
        var text = message.AddColorByType(type);
        
        // TODO: How to define originator and get player color?
        var originator = "ORIGINATOR".AddColor(Color.grey);
        Instantiate(_textMessagePrefab, _logTransform).SetMessage(
            new Message(originator, time, text)
        );
    }

    public void LogWithOriginator(string message, int originatorId, LogType type)
    {
        // Get timestamp for message
        var time = System.DateTime.Now.ToString("HH:mm:ss");
        Instantiate(_textMessagePrefab, _logTransform).SetMessage(
            new Message(originatorId.ToString(), time, message.AddColorByType(type))
        );
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