public class DeployState : CardInteractionState
{
    public override string ConfigName => "8_Deploy";
    public override string InteractionText 
    {
        get {
            if(numberSelections == 0) 
                return "You have no Plays available";
            else
                return "You may play a Creature card";
        }
    }

    public override bool CheckStateAutoskip() => !ContainsCreature() || numberSelections == 0;
    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(cardStats.cardInfo.type == CardType.Creature) return CardLocation.Selection;

        return null;
    }

    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable();

    internal override void CheckPlayability(int cash)
    {
        foreach (var card in selectableCards) {
            if (card.cardInfo.type != CardType.Creature) continue;

            card.CheckPlayability(cash);
        }
    }
} 