using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class CardDragManager : MonoBehaviour
{
    [SerializeField] private SortableCardPile _playerHand;
    [SerializeField] private SortableCardPile _selection;
    [SerializeField] private GameObject _cardSlotPrefab;

    [Header("Slot Pool")]
    [SerializeField] private int _prewarmCount = 10;
    public readonly Queue<CardSlot> _slotPool = new();
    public readonly Dictionary<int, CardSlot> activeSlots = new();

    public void AddCardToHand(CardsPileSors pile, GameObject card)
    {
        print("Card drag slot creation");

		var slot = GetSlot(card.GetComponent<CardStats>().cardInfo.goID);
        slot.transform.SetParent(pile.cardHolderTransform, false);
        slot.SetCard(card);

        _playerHand.AddSlot(slot.DragHandler);
    }

    internal void CardArrives(CardsPileSors pile, GameObject card)
    {
        if (pile.isSortable) AddCardToHand(pile, card);
    }

    internal void CardLeaves(CardsPileSors pile, GameObject card)
    {
        if (pile.isSortable) RemoveFromHand(card);
    }

    private void RemoveFromHand(GameObject card)
    {
        print("Remove card from card slot");

        var slot = activeSlots[card.GetComponent<CardStats>().cardInfo.goID];

        ReturnSlotToPool(slot);
        _playerHand.RemoveSlot(slot.DragHandler);
    }

    private void Awake()
    {
        // Prewarm slots
        for (int i = 0; i < _prewarmCount; i++)
        {
            var slot = CreateSlotInstance();
            ReturnSlotToPool(slot);
        }
    }

    private CardSlot CreateSlotInstance()
    {
        var slot = Instantiate(_cardSlotPrefab, transform, false);
        slot.SetActive(false);

        return slot.GetComponent<CardSlot>();
    }

    private CardSlot GetSlot(int id)
    {
        CardSlot slot;
        if (_slotPool.Count <= 0) slot = CreateSlotInstance();
        else slot = _slotPool.Dequeue();

        slot.transform.gameObject.SetActive(true);
        activeSlots[id] = slot;

        return slot;
    }

    private void ReturnSlotToPool(CardSlot slot)
    {
        var slotTransform = slot.DragHandler.transform.parent;
        slotTransform.gameObject.SetActive(false);
        slotTransform.SetParent(transform, false); // back under manager
        _slotPool.Enqueue(slot);
    }
}