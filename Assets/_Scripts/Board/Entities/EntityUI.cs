using System;
using UnityEngine;

public class EntityUI : CardUI
{
    public void InspectEntity(CardInfo card)
    {
        // TODO: Animate and combine with DetailCardUI ?
        SetCardUI(card, card.entitySpritePath);
        gameObject.SetActive(true);
    }

    internal void Hide()
    {   
        gameObject.SetActive(false);
    }

    public virtual void Highlight(HighlightType type)
    {
        if (type == HighlightType.None)
        {
            DisableHighlight();
            return;
        }

        var color = type switch
        {
            HighlightType.InteractionPositive => ColorPalette.interactionPositiveHighlight,
            HighlightType.InteractionNegative => ColorPalette.interactionNegativeHighlight,
            HighlightType.Selected => ColorPalette.selectedHighlight,
            HighlightType.Trigger => ColorPalette.triggerHighlight,
            HighlightType.Ability => ColorPalette.abilityHighlight,
            HighlightType.Target => ColorPalette.targetHighlight,
            _ => ColorPalette.defaultHighlight  
        };

        highlight.color = color;
        highlight.enabled = true;
        // highlight.color = color;
    }

    public void DisableHighlight()
    {
        if(highlight == null) return;

        highlight.enabled = false;
    }

    public void Attacking()
    {
        // highlight.color = colors.attacking;
    }

    public void Blocking()
    {
        // highlight.color = colors.blocking;
    }
}
