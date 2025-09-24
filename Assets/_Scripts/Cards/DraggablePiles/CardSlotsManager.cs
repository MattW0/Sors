using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardSlotsManager : MonoBehaviour 
{
    private ICardSlotFactory _factory;
    [SerializeField] private GameObject _slotPrefab;
    private CardMover _cardMover;

    private void Awake()
    {
        _factory = new CardSlotFactory(_slotPrefab, transform);
        _cardMover = ServiceLocator.Global.Get<CardMover>();
    }

    internal void Initialize(CardPile pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        var slot = SpawnSlot(card);

        // print($"Initialize card {stats.cardInfo.title} at pile {pile.pileType}");
        slot.SetParent(pile.CardPileTransformation.cardHolderTransform);
        pile.AddCard(slot.DragHandler, card);
    }

    internal void CardArrives(CardPile pile, GameObject card)
    {
        // print($"card {card.GetComponent<CardStats>().cardInfo.title} arrives at pile {pile.pileType}");
        
        var slot = card.GetComponentInParent<CardSlot>();
        pile.AddCard(slot.DragHandler, card);

        slot.SetParent(pile.CardPileTransformation.cardHolderTransform);
        slot.transform.localPosition = Vector3.zero;
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
}