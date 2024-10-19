using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.Utilities;

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
    private void StartTurn(int turnNumber)
    {
        turnText.text = "Turn " + turnNumber.ToString();
        _turnScreenOverlay.UpdateTurnScreen(turnNumber);
    }

    public void ChangeActionDescriptionText(TurnState state)
    {
        print("Changing action description in state " + state.ToString());

        if (state == TurnState.NextPhase) return;
        actionDescriptionText.text = GetText(state);

        // Only change icon and title when phase changes
        var iconPath = GetIconPath(state);
        if (iconPath.IsNullOrWhitespace()) return;
        
        phaseTitleText.text = state.ToString();
        if (state == TurnState.PhaseSelection) phaseTitleText.text = "Phase Selection";

        phaseIcon.sprite = Resources.Load<Sprite>(iconPath);
    }

    private string GetText(TurnState state)
    {
        return state switch {
            TurnState.PhaseSelection => "Select " + NumberPhases.ToString() + " phases",
            TurnState.Discard => "Discard cards",
            TurnState.Invent => "Buy technologies",
            TurnState.Develop => "Play technologies",
            TurnState.Attackers => "Declare attackers",
            TurnState.Blockers => "Declare blockers",
            TurnState.Recruit => "Buy creatures",
            TurnState.Deploy => "Play creatures",
            TurnState.Prevail => "Choose prevail options",
            TurnState.CardSelection => "Put cards into your hand",
            TurnState.Trash => "Trash cards from your hand",
            TurnState.CleanUp => "Clean up",
            _ => ""
        };
    }

    private string GetIconPath(TurnState state)
    {
        return state switch {
            TurnState.PhaseSelection => "Sprites/UI/Icons/Phases/phaseSelection",
            TurnState.Draw => "Sprites/UI/Icons/Phases/draw",
            TurnState.Invent => "Sprites/UI/Icons/Phases/buy",
            TurnState.Develop => "Sprites/UI/Icons/Phases/play",
            TurnState.Attackers => "Sprites/UI/Icons/Phases/attack",
            TurnState.Blockers => "Sprites/UI/Icons/Phases/defend",
            TurnState.Recruit => "Sprites/UI/Icons/Phases/buy",
            TurnState.Deploy => "Sprites/UI/Icons/Phases/play",
            TurnState.Prevail => "Sprites/UI/Icons/Phases/prevail",
            _ => ""
        };
    }
}
