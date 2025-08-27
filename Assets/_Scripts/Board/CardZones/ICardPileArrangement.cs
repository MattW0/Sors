using System.Collections.Generic;
using UnityEngine;

public interface ICardPileArrangement
{
    void AddCard(CardDragHandler dragHandler, GameObject card);
    void RemoveCard(CardDragHandler card);
}

[RequireComponent(typeof(CardsPileSors))]
public abstract class CardPileArrangement : MonoBehaviour, ICardPileArrangement
{
    public List<CardDragHandler> cards = new();
    public virtual void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        cards.Add(dragHandler);
        dragHandler.gameObject.transform.parent.gameObject.name = $"{cards.IndexOf(dragHandler)}";

        dragHandler.transform.localPosition = Vector3.zero;
        card.transform.localPosition = Vector3.zero;
    }

    public virtual void RemoveCard(CardDragHandler card)
    {
        cards.Remove(card);
    }
}
