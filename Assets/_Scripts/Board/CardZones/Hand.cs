using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using DG.Tweening;

public class Hand : NetworkBehaviour
{
    public static Hand Instance { get; private set; }
    private InteractionPanel _interactionPanel;
    [SerializeField]private List<CardStats> _handCards = new();
    public int HandCardsCount => _handCards.Count;
    private TurnState _state;
    [SerializeField] private Transform _cardHolder;
    private Vector3 _handPositionPlayCards = new Vector3(-200, 100, 0);
    private Vector3 _handScalePlayCards = new Vector3(1.2f, 1.2f, 0);

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start()
    {
        _interactionPanel = InteractionPanel.Instance;
    }
    
    [TargetRpc]
    public void TargetHighlightMoney(NetworkConnection target) => HighlightMoneyHandCards(true);

    private void HighlightAllHandCards(bool b)
    {
        foreach(var card in _handCards) card.IsInteractable = b;
    }

    private void HighlightMoneyHandCards(bool b)
    {
        foreach(var card in _handCards) if (card.cardInfo.type == CardType.Money) card.IsInteractable = b;
    }

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, int newAmount)
    {
        var allowedType = _state switch
        {
            TurnState.Develop => CardType.Technology,
            TurnState.Deploy => CardType.Creature,
            _ => CardType.None
        };

        foreach (var card in _handCards) {
            if (card.cardInfo.type != allowedType) continue;

            card.CheckPlayability(newAmount);
        }
    }

    public void StartInteraction(TurnState state)
    {
        _state = state;
        _cardHolder.DOLocalMove(_handPositionPlayCards, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_handScalePlayCards, SorsTimings.cardPileRearrangement);
        
        if (state == TurnState.Discard || state == TurnState.Trash) {
            HighlightAllHandCards(true);
            return;
        }

        HighlightMoneyHandCards(true);
    }

    public void EndInteraction()
    {
        _cardHolder.DOLocalMove(Vector3.zero, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
        HighlightAllHandCards(false);
    }

    public void UpdateHandCards(List<GameObject> cards, bool adding) 
    {
        for (int i = 0; i < cards.Count; i++) {
            var stats = cards[i].GetComponent<CardStats>();
        
            if (adding) _handCards.Add(stats);
            else _handCards.Remove(stats);
        }
    }

    public void RemoveCard(GameObject card) => _handCards.Remove(card.GetComponent<CardStats>());
    public bool ContainsMoney() => _handCards.Any(c => c.cardInfo.type == CardType.Money);
    public bool ContainsTechnology() => _handCards.Any(c => c.cardInfo.type == CardType.Technology);
    public bool ContainsCreature() => _handCards.Any(c => c.cardInfo.type == CardType.Creature);
}
