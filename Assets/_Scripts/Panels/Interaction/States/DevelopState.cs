using UnityEngine;
using System.Collections.Generic;

public class DevelopState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Develop";

    public override bool CheckStateAutoskip() => !ContainsTechnology();
    public override CardLocation? GetDestination(CardStats stats)
    {
        if(stats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(stats.cardInfo.type == CardType.Technology) return CardLocation.Selection;

        return null;
    }

    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards)
        {
            bool isInteractable = card.cardInfo.type == CardType.Technology;
            card.SetInteractable(isInteractable, config.turnState);
        }
    }

    // Up-to selection
    public override bool IsConfirmEnabled(int numberSelected) => numberSelected <= numberSelections;
} 