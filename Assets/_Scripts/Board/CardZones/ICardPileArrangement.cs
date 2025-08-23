using System.Collections.Generic;
using UnityEngine;

public interface ICardPileArrangement
{
    List<CardDragHandler> Cards { get; }
    void AddCard(CardDragHandler dragHandler, GameObject card);
    void RemoveCard(CardDragHandler card);
}
