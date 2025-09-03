using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;
using CardDecoder;

public class SortableCardPile : CardPileArrangement
{
    [SerializeField] private CardDragHandler _movingCard;
    [SerializeField] private bool tweenCardReturn = true;
    private bool _isCrossing = false;

    public override void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        base.AddCard(dragHandler, card);

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

        var endValue = _movingCard.selected ? new Vector3(0, _movingCard.selectionOffset ,0) : Vector3.zero;
        var duration = tweenCardReturn ? .15f : 0;

        _movingCard.transform.DOLocalMove(endValue, duration).SetEase(Ease.OutBack);
        _movingCard = null;

        // foreach(var c in cards) c.cardVisual.ResetShakeParent();
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
                Swap(i);
                break;
            }

            // Moving left
            if (movingX < otherX && movingIndex > otherIndex)
            {
                Swap(i);
                break;
            }
        }
    }

    private void Swap(int index)
    {
        _isCrossing = true;

        Transform focusedParent = _movingCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        bool swapIsRight = cards[index].ParentIndex() > _movingCard.ParentIndex();
        // cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        _isCrossing = false;
    }
}
