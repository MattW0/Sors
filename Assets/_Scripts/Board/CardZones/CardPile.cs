using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICardPile
{
    int NumberCards { get; }
    void AddCard(CardDragHandler dragHandler, GameObject card);
    void RemoveCard(CardDragHandler card);
    void StartInteraction();
    void EndInteraction();
}

public abstract class CardPile : MonoBehaviour, ICardPile
{
    public List<CardDragHandler> cards = new();
    public CardLocation pileType;
    [HideInInspector] public CardPileTransformation CardPileTransformation { get; private set; }
    public int NumberCards => cards.Count();

    private void Awake() 
    {
        CardPileTransformation = GetComponent<CardPileTransformation>();
    }

    public virtual void VerifyOrder()
    {
        var temp = transform.GetChild(0).GetComponentsInChildren<CardDragHandler>();

        // print(" --- Verify order ---");
        foreach(var dragHandler in temp){
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

        CardPileTransformation.StartTransform().Forget();
    }

    public virtual void RemoveCard(CardDragHandler card)
    {
        cards.Remove(card);
        CardPileTransformation.StartTransform().Forget();
    }

    public void StartInteraction() => CardPileTransformation.StartMove(true).Forget();
    public void EndInteraction() => CardPileTransformation.StartMove(false).Forget();
}
