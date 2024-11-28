public class EntityUI : CardUI
{
    public void InspectEntity(CardInfo card)
    {
        // TODO: Animate and combine with DetailCardUI ?
        SetCardUI(card, card.entitySpritePath, true);
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
            HighlightType.InteractionPositive => UIManager.ColorPalette.interactionPositiveHighlight,
            HighlightType.InteractionNegative => UIManager.ColorPalette.interactionNegativeHighlight,
            HighlightType.Selected => UIManager.ColorPalette.selectedHighlight,
            HighlightType.Trigger => UIManager.ColorPalette.triggerHighlight,
            HighlightType.Ability => UIManager.ColorPalette.abilityHighlight,
            HighlightType.Target => UIManager.ColorPalette.targetHighlight,
            _ => UIManager.ColorPalette.defaultHighlight  
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
