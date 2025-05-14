public class DevelopState : CardInteractionState
{
    public override string ConfigName => "3_Develop";
    public override string InteractionText {
        get {
            if(numberSelections == 0)
                return "You have no Plays available";
            else
                return "You may play a Technology card";
        }
    }

    public DevelopState()
    {
        PlayerManager.OnLocalCashUpdate += CheckPlayability;
    }

    public override bool CheckStateAutoskip() => !SelectablesContainTechnology();
    public override CardLocation? GetCardDestination(CardStats stats)
    {
        if(stats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(stats.cardInfo.type == CardType.Technology) return CardLocation.Selection;

        return null;
    }
    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable();

    ~DevelopState() 
    {
        PlayerManager.OnLocalCashUpdate -= CheckPlayability;
    }
} 