public class InventState : CardInteractionState
{
    public override string ConfigName => "2_Invent";

    public override string InteractionText {
        get {
            if(numberSelections == 0) 
                return "You have no Buys available";
            else
                return "Buy a Technology or Money card";
        }
    }

    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        return null;
    }
    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCashSpending(true);
    public override void HandleReset(InteractionPanel ctx) => ctx.UndoMoneyPlay();
    public override void HandleSkip(InteractionPanel ctx) => ctx.SkipInteraction();
    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable();
}
