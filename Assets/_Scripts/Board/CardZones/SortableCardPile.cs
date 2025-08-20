using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

public class SortableCardPile : MonoBehaviour
{
    [SerializeField] private CardDragHandler _movingCard;
    public List<CardDragHandler> _draggableCards;
    [SerializeField] private bool tweenCardReturn = true;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 0;
    [SerializeField] private GameObject slotPrefab;
    private bool _isCrossing = false;

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

    internal void RemoveSlot(CardDragHandler dragHandler)
    {
        _draggableCards.Remove(dragHandler);
        dragHandler.BeginDragEvent.RemoveListener(BeginDrag);
        dragHandler.EndDragEvent.RemoveListener(EndDrag);
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

        for (int i = 0; i < _draggableCards.Count; i++)
        {
            // if (i == movingIndex) continue;

            float otherX = _draggableCards[i].cardVisual.transform.position.x;
            var otherIndex = _draggableCards[i].ParentIndex();

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
        Transform crossedParent = _draggableCards[index].transform.parent;

        _draggableCards[index].transform.SetParent(focusedParent);
        _draggableCards[index].transform.localPosition = _draggableCards[index].selected ? new Vector3(0, _draggableCards[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        bool swapIsRight = _draggableCards[index].ParentIndex() > _movingCard.ParentIndex();
        _draggableCards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        _isCrossing = false;
        // StartCoroutine(Frame());
    }

    // private IEnumerator Frame()
    // {
    //     yield return new WaitForSecondsRealtime(.1f);
    //     _isCrossing = false;
    // }
}
