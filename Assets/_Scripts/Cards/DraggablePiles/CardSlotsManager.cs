using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardSlotsManager : MonoBehaviour 
{
    private Dictionary<CardsPileSors, ICardPileArrangement> _controllers = new();
    // private readonly Dictionary<int, CardSlot> _activeSlots = new();
    private ICardSlotFactory _factory;
    [SerializeField] private GameObject _slotPrefab;
    private CardMover _cardMover;

    private void Awake()
    {
        _factory = new CardSlotFactory(_slotPrefab, transform);
        _cardMover = ServiceLocator.Global.Get<CardMover>();

        foreach (var pile in _cardMover.GetPiles())
            _controllers[pile] = pile.GetComponent<ICardPileArrangement>();
    }

    internal void Initialize(CardsPileSors pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        var slot = SpawnSlot(card);

        print($"Initialize card {stats.cardInfo.title} at pile {pile.pileType}");
        slot.SetParent(pile.cardHolderTransform);
        _controllers[pile].AddCard(slot.DragHandler, card);
    }

    internal void CardArrives(CardsPileSors pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        var slot = card.GetComponentInParent<CardSlot>();

        print($"card {stats.cardInfo.title} arrives at pile {pile.pileType}");
        slot.SetParent(pile.cardHolderTransform);
        _controllers[pile].AddCard(slot.DragHandler, card);
    }

    internal void CardLeaves(CardsPileSors pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        
        print($"Remove {stats.cardInfo.title} from collection {pile.pileType}");
        // var slot = _activeSlots[stats.cardInfo.goID];
        _controllers[pile].RemoveCard(card.GetComponentInParent<CardSlot>().DragHandler);
    }
    
    private CardSlot SpawnSlot(GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        print("Spawning slot for card " + stats.cardInfo.title);
        var slot = _factory.CreateSlot();

        slot.Initialize(stats);
        // await UniTask.Delay(SorsTimings.spawnCard);

        return slot;
    }
}