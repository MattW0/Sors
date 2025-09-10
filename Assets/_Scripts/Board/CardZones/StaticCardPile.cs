using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;


public class StaticCardPile : CardPileArrangement
{
    public override void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        base.AddCard(dragHandler, card);
        dragHandler.Draggable = false;
    }
}

