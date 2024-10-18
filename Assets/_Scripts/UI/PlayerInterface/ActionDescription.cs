using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text actionDescriptionText;
    [SerializeField] private Image phaseIcon;
    [SerializeField] private TMP_Text phaseTitleText;
    [SerializeField] private TurnScreenOverlay _turnScreenOverlay;
    public int NumberPhases { get; set; }

    private void Awake()
    {
        PhasePanel.OnPhaseSelectionStarted += StartTurn;
    }

    public void ChangeActionDescriptionText(TurnState state)
    {
        if (state == TurnState.NextPhase) return;

        actionDescriptionText.text = GetText(state);

        if (state == TurnState.Discard || state == TurnState.CardSelection || state == TurnState.Trash) return;

        var iconPath = GetIconPath(state);
        var title = state.ToString();
        if (state == TurnState.PhaseSelection) title = "Phase Selection";

        phaseTitleText.text = title;
        phaseIcon.sprite = Resources.Load<Sprite>(iconPath);
    }

    private void StartTurn(int turnNumber)
    {
        turnText.text = "Turn " + turnNumber.ToString();
        _turnScreenOverlay.UpdateTurnScreen(turnNumber);
    }

    private string GetText(TurnState state)
    {
        // TODO: Clean up this stuff... UI elements in playerInterface should have its own class
        // and this is hidden in PhasePanel context.
        // Make it listen to turnManager.OnPhaseChanged ?
        return state switch {
            TurnState.PhaseSelection => "Select " + NumberPhases.ToString() + " phases",
            TurnState.Discard => "Discard cards",
            TurnState.Invent => "Buy technologies",
            TurnState.Develop => "Play technologies",
            TurnState.Recruit => "Buy creatures",
            TurnState.Deploy => "Play creatures",
            TurnState.Prevail => "Choose prevail options",
            TurnState.CardSelection => "Put a card in your hand",
            TurnState.Trash => "Trash cards",
            _ => ""
        };
    }

    private string GetIconPath(TurnState state)
    {
        return state switch {
            TurnState.PhaseSelection => "Sprites/UI/Icons/Phases/flag",
            TurnState.Draw => "Sprites/UI/Icons/Phases/cards",
            TurnState.Invent => "Sprites/UI/Icons/Phases/pouch",
            TurnState.Develop => "Sprites/UI/Icons/Phases/forward",
            TurnState.Attackers => "Sprites/UI/Icons/Phases/sword",
            TurnState.Blockers => "Sprites/UI/Icons/Phases/shield",
            TurnState.Recruit => "Sprites/UI/Icons/Phases/pouch",
            TurnState.Deploy => "Sprites/UI/Icons/Phases/forward",
            TurnState.Prevail => "Sprites/UI/Icons/Phases/idea",
            _ => "Sprites/UI/Icons/Phases/flag.png"
        };
    }
}
