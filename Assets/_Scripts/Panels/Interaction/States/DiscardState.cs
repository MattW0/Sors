using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DiscardState : InteractionStateBase
{
    public override string ConfigName => "TurnStates/Discard";
    public override bool CheckStateAutoskip() => false;
    public override CardLocation? GetCardDestination(CardStats cardStats) => CardLocation.Selection;
    public override void MakeCardsInteractable(List<CardStats> cards)
    {
        foreach(var card in cards) card.SetInteractable(true, config.turnState);
    }
}
