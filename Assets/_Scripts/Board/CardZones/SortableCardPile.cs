using UnityEngine;
using DG.Tweening;

public class SortableCardPile : CardPile
{
    [SerializeField] private CardDragHandler _movingCard;
    [SerializeField] private bool tweenCardReturn = true;
    private bool _isCrossing = false;
    
    public override void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        base.AddCard(dragHandler, card);

        dragHandler.Draggable = true;
        dragHandler.BeginDragEvent.AddListener(BeginDrag);
        dragHandler.EndDragEvent.AddListener(EndDrag);
    }

    public override void RemoveCard(CardDragHandler card)
    {
        base.RemoveCard(card);
        
        card.BeginDragEvent.RemoveListener(BeginDrag);
        card.EndDragEvent.RemoveListener(EndDrag);
    }

    private void BeginDrag(CardDragHandler card) => _movingCard = card;
    void EndDrag(CardDragHandler card)
    {
        if (_movingCard == null) return;

        var duration = tweenCardReturn ? .15f : 0;

        _movingCard.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutBack);
        _movingCard = null;
    }

    void LateUpdate()
    {
        if (_movingCard == null || _isCrossing) return;
        CheckSlotPosition();
    }

    private void CheckSlotPosition()
    {
        float movingX = _movingCard.cardVisual.transform.position.x;
        int movingIndex = _movingCard.ParentIndex();

        for (int i = 0; i < cards.Count; i++)
        {
            // if (i == movingIndex) continue;

            float otherX = cards[i].cardVisual.transform.position.x;
            var otherIndex = cards[i].ParentIndex();

            // Moving right
            if (movingX > otherX && movingIndex < otherIndex)
            {
                Swap(cards[i]);
                break;
            }

            // Moving left
            if (movingX < otherX && movingIndex > otherIndex)
            {
                Swap(cards[i]);
                break;
            }
        }
    }

    private void Swap(CardDragHandler card)
    {
        _isCrossing = true;

        // Set new parent
        Transform t_from = _movingCard.transform.parent;
        Transform t_to = card.transform.parent;

        card.transform.SetParent(t_from);
        card.transform.localPosition = Vector3.zero;
        _movingCard.transform.SetParent(t_to);

        VerifyOrder();
        _isCrossing = false;
    }
}
