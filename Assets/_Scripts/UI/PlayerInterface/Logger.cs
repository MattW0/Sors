using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Sirenix.Utilities;
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

    public void StartGame(string[] names) => Log($" --- {names[1]} vs {names[2]} --- ", names[0], LogType.Standard); 
    public void TurnStart(string originator, int turnNumber) => Log($"Turn {turnNumber}", originator, LogType.TurnChange);
    public void EndGame(string originator) => Log("Wins the game!", originator, LogType.Standard);
    public void PhasesToPlay(string originator, List<TurnState> phases)
    {
        var msg = $"Phases to play:";
        msg += string.Join(", ", phases.Select(phase => phase.ToString()));

        Log(msg, originator, LogType.Standard);
    }

    public void PhaseChange(string originator, TurnState phase)
    {
        var text = phase.ToString();
        if (phase == TurnState.Attackers) text = "Combat";

        Log(text, originator, LogType.Phase);
    }

    public void PlayerTargeting(string originator, string sourceTitle, string targetTitle, LogType type)
    {
        if (type == LogType.CombatAttacker) Log($"{sourceTitle} attacks {targetTitle}", originator, LogType.CombatAttacker);
        else if (type == LogType.CombatBlocker) Log($"{sourceTitle} blocks {targetTitle}", originator, LogType.CombatBlocker);
        else if (type == LogType.AbilityTarget) Log($"{sourceTitle} targets {targetTitle}", originator, LogType.CombatClash);
        else if (type == LogType.AbilityExecution) Log($"Executing ability from {sourceTitle} with target {targetTitle}", originator, LogType.AbilityExecution);
    }

    public void PlayerSpendsCash(string originator, string cardName, int cost, LogType type)
    {
        if(type == LogType.Buy) Log($"Buys {cardName} for {cost} cash", originator, LogType.Buy);
        else if(type == LogType.Play) Log($"Plays {cardName} for {cost} cash", originator, LogType.Play);
    }

    public void PlayerDrawsCards(string originator, int number) => Log($"Draws {number} cards", originator, LogType.Standard);

    public void PlayerDiscardsCards(string originator, List<string> cardNames)
    {
        var msg = "Discards cards: ";
        msg += string.Join(", ", cardNames);
        Log(msg, originator, LogType.Standard);
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