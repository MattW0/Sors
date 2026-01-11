public class DiscardState : CardInteractionState
{
    public override string ConfigName => "1_Discard";
    public override string InteractionText {
        get {
            if(numberSelections == 1) 
                return "Discard a card";
            else
                return $"Discard {numberSelections} cards";
        }
    }
    public override CardLocation? GetCardDestination(CardStats cardStats) => CardLocation.Selection;

    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCardSelection();
    public override void HandleReset(InteractionPanel ctx) => ctx.UndoMoneyPlay();
    public override void HandleSkip(InteractionPanel ctx){ }

    public override void MakeCardsInteractable() => MakeAllCardsInteractable();
}
