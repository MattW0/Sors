using UnityEngine;
using System.Collections.Generic;

public class DeployInteractionState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Deploy";

    public override bool CheckStateSpecificAutoskip()
    {
        return !ContainsCreature();
    }


    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards)
        {
            bool isInteractable = card.cardInfo.type == CardType.Creature;
            card.SetInteractable(isInteractable, config.turnState);
        }
    }
} 