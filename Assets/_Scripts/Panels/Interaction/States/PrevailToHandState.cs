using System.Collections.Generic;
using UnityEngine;

public class PrevailToHandState : InteractionStateBase
{
    public override string ConfigName => "9_PrevailToHand";
    public override string InteractionText {
        get {
            if(numberSelections == 1) 
                return "Return a card from your discard to your hand";
            else
                return $"Return up to {numberSelections} cards from your discard to your hand";
        }
    }

    public override bool CheckStateAutoskip() => numberSelections == 0;
    public override CardLocation? GetCardDestination(CardStats cardStats) => CardLocation.Selection;
    public override void MakeCardsInteractable() => MakeAllCardsInteractable();
}
