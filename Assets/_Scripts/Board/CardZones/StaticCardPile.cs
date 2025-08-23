using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardsPileSors))]
public class StaticCardPile : MonoBehaviour, ICardPileArrangement
{
    private List<CardDragHandler> _cards = new();
    public List<CardDragHandler> Cards => _cards;

    public void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        _cards.Add(dragHandler);
        card.gameObject.name = $"{_cards.IndexOf(dragHandler)}";
        card.transform.localPosition = Vector3.zero;
    }

    public void RemoveCard(CardDragHandler card)
    {
        _cards.Remove(card);
    }
}

