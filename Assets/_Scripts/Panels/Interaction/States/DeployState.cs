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

    public DeployState()
    {
        PlayerManager.OnLocalCashUpdate += CheckPlayability;
    }

    public override bool CheckStateAutoskip() => !SelectablesContainCreature();
    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(cardStats.cardInfo.type == CardType.Creature) return CardLocation.Selection;

        return null;
    }

    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCashSpending(false);
    public override void HandleReset(InteractionPanel ctx) => ctx.UndoMoneyPlay();
    public override void HandleSkip(InteractionPanel ctx) => ctx.SkipInteraction();

    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable(Config.turnState);
    
    ~DeployState() 
    {
        PlayerManager.OnLocalCashUpdate -= CheckPlayability;
    }
} 