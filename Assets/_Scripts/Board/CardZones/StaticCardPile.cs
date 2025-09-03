using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;


public class StaticCardPile : CardPileArrangement
{
    public override void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        base.AddCard(dragHandler, card);
        dragHandler.MakeStatic();
    }

    public override void RemoveCard(CardDragHandler card)
    {
        base.RemoveCard(card);
        card.MakeSortable();
    }
}

