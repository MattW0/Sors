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
        dragHandler.gameObject.name = $"{_draggableCards.IndexOf(dragHandler)}";

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
        float movingX = _movingCard.cardVisual.transform.position.x;
        // int movingIndex = _draggableCards.IndexOf(_movingCard);

        for (int i = 0; i < _draggableCards.Count; i++)
        {
            // if (i == movingIndex) continue;

            float otherX = _draggableCards[i].cardVisual.transform.position.x;

            // Moving right
            if (movingX > otherX) //movingIndex < i &&
            {
                if (_movingCard.ParentIndex() < _draggableCards[i].ParentIndex())
                {
                Swap(i);
                break;
                }
            }

            // Moving left
            if (movingX < otherX)
            {
                if (_movingCard.ParentIndex() > _draggableCards[i].ParentIndex())
                {
                Swap(i);
                break;
                }
            }
        }
    }

    private void Swap(int index)
    {
        // print($"Swap index : {fromIndex} -> {toIndex}");
        isCrossing = true;

        // Get slots from the cards
        // Transform slotA = _draggableCards[fromIndex].transform.parent;
        // Transform slotB = _draggableCards[toIndex].transform.parent;

        // // Swap slots in hierarchy
        // slotA.SetSiblingIndex(toIndex);
        // slotB.SetSiblingIndex(fromIndex);

        // // Swap in list
        // (_draggableCards[toIndex], _draggableCards[fromIndex]) = (_draggableCards[fromIndex], _draggableCards[toIndex]);

        Transform focusedParent = _movingCard.transform.parent;
        Transform crossedParent = _draggableCards[index].transform.parent;

        _draggableCards[index].transform.SetParent(focusedParent);
        _draggableCards[index].transform.localPosition = _draggableCards[index].selected ? new Vector3(0, _draggableCards[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        bool swapIsRight = _draggableCards[index].ParentIndex() > _movingCard.ParentIndex();
        _draggableCards[index].cardVisual.Swap(swapIsRight ? -1 : 1);



        // int swapDirection = toIndex > fromIndex ? -1 : 1;
        // _draggableCards[toIndex].Swap(swapDirection);

        StartCoroutine(Frame());
    }

    private IEnumerator Frame()
    {
        yield return new WaitForSecondsRealtime(.1f);
        isCrossing = false;
    }
}
