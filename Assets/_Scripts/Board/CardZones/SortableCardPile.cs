using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

public class SortableCardPile : MonoBehaviour
{
    [SerializeField] private CardDragHandler _movingCard;
    [SerializeField] private GameObject slotPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<CardDragHandler> _draggableCards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    void Start()
    {
        for (int i = 0; i < cardsToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        foreach (CardDragHandler dragHandler in GetComponentsInChildren<CardDragHandler>().ToList()) AddSlot(dragHandler);
    }

    internal void AddSlot(CardDragHandler dragHandler)
    {
        _draggableCards.Add(dragHandler);
        dragHandler.BeginDragEvent.AddListener(BeginDrag);
        dragHandler.EndDragEvent.AddListener(EndDrag);
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
        if (_movingCard == null || isCrossing) return;
        CheckSlotPosition();
    }

    private void CheckSlotPosition()
    {
        float movingX = _movingCard.transform.position.x;
        int movingIndex = _draggableCards.IndexOf(_movingCard);

        for (int i = 0; i < _draggableCards.Count; i++)
        {
            if (i == movingIndex) continue;
            float otherX = _draggableCards[i].transform.position.x;

            // Moving right
            if (movingX > otherX && movingIndex < i)
            {
                Swap(movingIndex, i);
                break;
            }

            // Moving left
            if (movingX < otherX && movingIndex > i)
            {
                Swap(movingIndex, i);
                break;
            }
        }
    }

    private void Swap(int fromIndex, int toIndex)
    {
        isCrossing = true;

        var moving = _draggableCards[fromIndex];
        var target = _draggableCards[toIndex];

        target.transform.SetParent(moving.transform.parent, false);
        target.transform.localPosition = target.selected ? new Vector3(0, target.selectionOffset, 0) : Vector3.zero;
        
        moving.transform.SetParent(target.transform.parent);

        _draggableCards[fromIndex] = target;
        _draggableCards[toIndex] = moving;

        int swapDirection = toIndex > fromIndex ? -1 : 1;
        target.Swap(swapDirection);

        isCrossing = false;
    }
}
