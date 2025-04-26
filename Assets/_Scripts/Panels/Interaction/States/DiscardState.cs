using System.Collections.Generic;
using UnityEngine;

public class DiscardState : InteractionStateBase
{
    public override string ConfigName => "DiscardInteraction";

    public override bool CheckStateSpecificAutoskip() => false;
    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards) card.SetInteractable(true, config.turnState);
    }
}
