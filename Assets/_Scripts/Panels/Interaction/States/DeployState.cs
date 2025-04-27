using UnityEngine;
using System.Collections.Generic;

public class DeployState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Deploy";

    public override bool CheckStateAutoskip() => !ContainsCreature();
    public override CardLocation? GetDestination(CardStats stats)
    {
        if(stats.cardInfo.type == CardType.Money) return CardLocation.MoneyZone;
        if(stats.cardInfo.type == CardType.Creature) return CardLocation.Selection;

        return null;
    }

    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards)
        {
            bool isInteractable = card.cardInfo.type == CardType.Creature;
            card.SetInteractable(isInteractable, config.turnState);
        }
    }

    // Up-to selection
    public override bool IsConfirmEnabled(int numberSelected) => numberSelected <= numberSelections;
} 