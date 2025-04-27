using UnityEngine;
using System.Collections.Generic;

public class DevelopInteractionState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Develop";

    public override bool CheckStateAutoskip(int numberSelections)
    {
        return !ContainsTechnology();
    }

    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards)
        {
            bool isInteractable = card.cardInfo.type == CardType.Technology;
            card.SetInteractable(isInteractable, config.turnState);
        }
    }
} 