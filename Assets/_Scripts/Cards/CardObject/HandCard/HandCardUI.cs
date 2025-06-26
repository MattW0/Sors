using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HandCardUI : CardUI
{
    [SerializeField] private GameObject _front;
    private CardVisualHandler visualHandler;


    public void CardBackUp()
    {
        _front.SetActive(false);
    }
    public void CardFrontUp()
    {
        _front.SetActive(true);
    }

    public void Highlight(bool value, TurnState state)
    {
        if (!value || state == TurnState.None) {
            highlight.enabled = false;
            return;
        }

        var color = state switch
        {
            // TurnState.Develop or TurnState.Deploy => ColorPalette.interactionPositiveHighlight,
            TurnState.Trash or TurnState.Discard => UIManager.ColorPalette.interactionNegativeHighlight,
            TurnState.CardSelection => UIManager.ColorPalette.interactionPositiveHighlight,
            _ => UIManager.ColorPalette.defaultHighlight
        };

        highlight.color = color;
        highlight.enabled = true;
    }

    public void Highlight(HighlightType type)
    {
        if (type == HighlightType.None)
        {
            highlight.enabled = false;
            return;
        }

        highlight.color = type switch
        {
            HighlightType.Playable => UIManager.ColorPalette.interactionPositiveHighlight,
            HighlightType.Selected => UIManager.ColorPalette.defaultHighlight,
            _ => UIManager.ColorPalette.defaultHighlight
        };
        highlight.enabled = true;
    }
}
