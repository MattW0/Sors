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

    public InventState()
    {
        InteractionPanel.OnUndoMoneyPlay += MakeMoneyCardsInteractable;
    }

    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        return null;
    }

    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable(Config.turnState);

    ~InventState() 
    {
        InteractionPanel.OnUndoMoneyPlay -= MakeMoneyCardsInteractable;
    }
}
