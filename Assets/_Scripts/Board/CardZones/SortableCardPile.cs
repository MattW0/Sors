using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

[RequireComponent(typeof(CardsPileSors))]
public class SortableCardPile : MonoBehaviour, ICardPileArrangement
{
    [SerializeField] private CardDragHandler _movingCard;
    [SerializeField] private bool tweenCardReturn = true;
    private bool _isCrossing = false;
    public List<CardDragHandler> Cards => _cards;
    private List<CardDragHandler> _cards = new();

    public void AddCard(CardDragHandler dragHandler, GameObject card)
    {
        _cards.Add(dragHandler);
        dragHandler.gameObject.name = $"{_cards.IndexOf(dragHandler)}";

        dragHandler.BeginDragEvent.AddListener(BeginDrag);
        dragHandler.EndDragEvent.AddListener(EndDrag);

        card.transform.localPosition = Vector3.zero;
    }

    public void RemoveCard(CardDragHandler card)
    {
        _cards.Remove(card);
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

        for (int i = 0; i < _cards.Count; i++)
        {
            // if (i == movingIndex) continue;

            float otherX = _cards[i].cardVisual.transform.position.x;
            var otherIndex = _cards[i].ParentIndex();

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
        Transform crossedParent = _cards[index].transform.parent;

        _cards[index].transform.SetParent(focusedParent);
        _cards[index].transform.localPosition = _cards[index].selected ? new Vector3(0, _cards[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        bool swapIsRight = _cards[index].ParentIndex() > _movingCard.ParentIndex();
        _cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        _isCrossing = false;
    }
}
