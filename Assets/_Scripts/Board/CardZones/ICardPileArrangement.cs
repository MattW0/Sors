using System.Collections.Generic;
using System.Linq;
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

    public virtual void VerifyOrder()
    {
        var temp = transform.GetChild(0).GetComponentsInChildren<CardDragHandler>();

        // print(" --- Verify order ---");
        foreach(var dragHandler in temp){
            // print(dragHandler.Stats.cardInfo.title);

            var parentTransform = dragHandler.gameObject.transform.parent;
            parentTransform.gameObject.name = $"{parentTransform.GetSiblingIndex()}";
        }

        cards = temp.ToList();
    }

    public virtual void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        cards.Add(dragHandler);

        dragHandler.transform.localPosition = Vector3.zero;
        card.transform.localPosition = Vector3.zero;
    }

    public virtual void RemoveCard(CardDragHandler card)
    {
        cards.Remove(card);
    }
}
