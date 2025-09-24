using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICardPile
{
    void AddCard(CardDragHandler dragHandler, GameObject card);
    void RemoveCard(CardDragHandler card);
    void StartInteraction();
    void EndInteraction();
}

public abstract class CardPile : MonoBehaviour, ICardPile
{
    public List<CardDragHandler> cards = new();
    public CardLocation pileType;
    [HideInInspector]public CardPileTransformation CardPileTransformation { get; private set; }

    private void Awake() 
    {
        CardPileTransformation = GetComponent<CardPileTransformation>();
    }

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

        card.transform.localPosition = Vector3.zero;
        dragHandler.ResetPosition();
    }

    public virtual void RemoveCard(CardDragHandler card)
    {
        cards.Remove(card);
    }

    public void StartInteraction() => CardPileTransformation.StartInteraction();
    public void EndInteraction() => CardPileTransformation.EndInteraction();
}
