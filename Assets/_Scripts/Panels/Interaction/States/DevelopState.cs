using UnityEngine;
using System.Collections.Generic;

public class DevelopState : InteractionStateBase
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

    public override bool CheckStateAutoskip() => !ContainsTechnology() || numberSelections == 0;
    public override CardLocation? GetCardDestination(CardStats stats)
    {
        if(stats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(stats.cardInfo.type == CardType.Technology) return CardLocation.Selection;

        return null;
    }
    public override void MakeCardsInteractable() => MakeMoneyCardsInteractable();
} 