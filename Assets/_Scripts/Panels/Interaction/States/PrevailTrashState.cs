public class PrevailTrashState : CardInteractionState
{
    public override string ConfigName => "10_PrevailTrash";
    public override string InteractionText {
        get {
            if(numberSelections == 1) 
                return "You may trash a card from your hand";
            else
                return $"Trash up to {numberSelections} cards from your hand";
        }
    }
    public override CardLocation? GetCardDestination(CardStats cardStats)  => CardLocation.Selection;
    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCardSelection();
    public override void HandleReset(InteractionPanel ctx) {}
    public override void HandleSkip(InteractionPanel ctx) => ctx.SkipInteraction();
    public override void MakeCardsInteractable() => MakeAllCardsInteractable();
}
