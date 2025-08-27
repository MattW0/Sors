using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CardSlotsManager : MonoBehaviour 
{
    private Dictionary<CardsPileSors, ICardPileArrangement> _controllers = new();
    private readonly Dictionary<int, CardSlot> _activeSlots = new();
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

    internal async UniTask CardArrives(CardsPileSors pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();

        if (! _activeSlots.TryGetValue(stats.cardInfo.goID, out var slot)) {
            slot = await SpawnSlot(card);
            // print("Spawn slot for card: " + stats.cardInfo.title);
        }

        // print($"card {stats.cardInfo.title} arrives at pile {pile.pileType}");
        slot.SetParent(pile.cardHolderTransform);
        _controllers[pile].AddCard(slot.DragHandler, card);
    }

    internal void CardLeaves(CardsPileSors pile, GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        
        // print($"Remove {stats.cardInfo.title} from collection {pile.pileType}");
        var slot = _activeSlots[stats.cardInfo.goID];
        _controllers[pile].RemoveCard(slot.DragHandler);
    }
    
    private async UniTask<CardSlot> SpawnSlot(GameObject card)
    {
        var stats = card.GetComponent<CardStats>();
        var slot = _factory.CreateSlot();

        slot.Initialize(card);
        _activeSlots[stats.cardInfo.goID] = slot;

        // Example: small spawn delay / animation
        await UniTask.Delay(SorsTimings.spawnCard);

        return slot;
    }
}