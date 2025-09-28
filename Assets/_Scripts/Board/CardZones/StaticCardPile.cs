using UnityEngine;


public class StaticCardPile : CardPile
{
    public override void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        base.AddCard(dragHandler, card);
        dragHandler.Draggable = false;

        VerifyOrder();
    }
}

