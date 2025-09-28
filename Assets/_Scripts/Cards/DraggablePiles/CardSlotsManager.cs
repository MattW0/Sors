using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardSlotsManager : MonoBehaviour 
{
    private ICardSlotFactory _factory;
    [SerializeField] private GameObject _slotPrefab;

    private void Awake() => _factory = new CardSlotFactory(_slotPrefab, transform);

    internal void Initialize(CardPile pile, GameObject card)
    {
        // print($"Initialize card {card.GetComponent<CardStats>().cardInfo.title} at pile {pile.pileType}");
        var slot = SpawnSlot(card);
        SetParent(pile, card, slot);
    }

    internal void CardArrives(CardPile pile, GameObject card)
    {
        // print($"card {card.GetComponent<CardStats>().cardInfo.title} arrives at pile {pile.pileType}");
        var slot = card.GetComponentInParent<CardSlot>();
        SetParent(pile, card, slot);
    }

    internal void CardLeaves(CardPile pile, GameObject card)
    {
        // print($"Remove {card.GetComponent<CardStats>().cardInfo.title} from collection {pile.pileType}");
        pile.RemoveCard(card.GetComponentInParent<CardSlot>().DragHandler);
    }
    
    private CardSlot SpawnSlot(GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        // print("Spawning slot for card " + stats.cardInfo.title);
        var slot = _factory.CreateSlot();

        slot.Initialize(stats);
        // await UniTask.Delay(SorsTimings.spawnCard);

        return slot;
    }

    private static void SetParent(CardPile pile, GameObject card, CardSlot slot)
    {
        pile.AddCard(slot.DragHandler, card);
        slot.SetParent(pile.CardPileTransformation.cardHolderTransform);
    }
}