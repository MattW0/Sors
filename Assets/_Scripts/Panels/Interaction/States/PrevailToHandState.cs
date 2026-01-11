public class PrevailToHandState : CardInteractionState
{
    public override string ConfigName => "9_PrevailToHand";
    public override string InteractionText {
        get {
            if(numberSelections == 1) 
                return "Return a card from your discard to your hand";
            else
                return $"Return up to {numberSelections} cards from your discard to your hand";
        }
    }
    public override CardLocation? GetCardDestination(CardStats cardStats) => CardLocation.Selection;
    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCardSelection();
    public override void HandleReset(InteractionPanel ctx) {}
    public override void HandleSkip(InteractionPanel ctx) => ctx.SkipInteraction();
    public override void MakeCardsInteractable() => MakeAllCardsInteractable(UIManager.ColorPalette.interactionPositiveHighlight);
}
