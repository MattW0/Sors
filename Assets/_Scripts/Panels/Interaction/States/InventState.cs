using System.Collections.Generic;
using UnityEngine;

public class InventState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Invent";
    public override bool CheckStateAutoskip() => false;
    public override CardLocation? GetCardDestination(CardStats cardStats)
    {
        if(cardStats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        return null;
    }

    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards)
        {
            bool isInteractable = card.cardInfo.type == CardType.Money;
            card.SetInteractable(isInteractable, config.turnState);
        }
    }
}
