using UnityEngine;

public class PrevailTrashState : InteractionStateBase
{
    public override string ConfigName => "10_PrevailTrash";
    public override string InteractionText {
        get {
            if(numberSelections == 1) 
                return "You may trash a card from your hand";
            else
                return $"Trash up to {numberSelections} cards from your hand";
        }
    }

    public override bool CheckStateAutoskip() => numberSelections == 0;
    public override CardLocation? GetCardDestination(CardStats cardStats)  => CardLocation.Selection;
    public override void MakeCardsInteractable() => MakeAllCardsInteractable();
}
