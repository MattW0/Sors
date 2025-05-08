public class RecruitState : CardInteractionState
{
    public override string ConfigName => "7_Recruit";
    public override string InteractionText {
        get {
            if(numberSelections == 0) 
                return "You have no Buys available";
            else
                return "Buy a Creature or Money card";
        }
    }
    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        return null;
    }
    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable();
}
