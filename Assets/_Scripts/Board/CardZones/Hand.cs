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
    private List<CardStats> _handCards = new();
    public void SetHandCards(List<CardStats> handCards) => _handCards = handCards;
    [SerializeField] private Transform _cardHolder;
    private Vector3 _handPositionPlayCards = new Vector3(-200, 100, 0);
    private Vector3 _handScalePlayCards = new Vector3(1.2f, 1.2f, 0);
    private TurnState _state;

    private void Awake()
    {
        if (!Instance) Instance = this;
        PlayerManager.OnCashChanged += PlayerCashChanged;
    }

    public void UpdateHandCardList(GameObject card, bool addingCard)
    {
        var stats = card.GetComponent<CardStats>();
        
        if (addingCard) _handCards.Add(stats);
        else _handCards.Remove(stats);
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) => HighlightCardTypes(isInteractable, new List<CardType> { CardType.Money });
    
    [TargetRpc]
    public void TargetHighlightMoney(NetworkConnection target) => HighlightCardTypes(true, new List<CardType> { CardType.Money });

    private void HighlightCardTypes(bool b, List<CardType> cardTypes)
    {
        foreach (var card in _handCards.Where(card => cardTypes.Contains(card.cardInfo.type))) card.IsInteractable = b;
    }

    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        if(_state == TurnState.Develop || _state == TurnState.Deploy)
            TargetCheckPlayability(player.connectionToClient, newAmount);
    }

    [TargetRpc]
    private void TargetCheckPlayability(NetworkConnection target, int newAmount)
    {
        foreach (var card in _handCards) card.CheckPlayability(newAmount);
    }

    public void StartInteraction(TurnState state)
    {
        _state = state;
        _cardHolder.DOLocalMove(_handPositionPlayCards, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_handScalePlayCards, SorsTimings.cardPileRearrangement);
        
        List<CardType> interactableCardTypes = new(); 
        if (state == TurnState.Develop || state == TurnState.Deploy)
        {
            interactableCardTypes.Add(CardType.Money);
            // if (state == TurnState.Develop) interactableCardTypes.Add(CardType.Technology);
            // else if (state == TurnState.Deploy) interactableCardTypes.Add(CardType.Creature);
        }

        HighlightCardTypes(true, interactableCardTypes);


    }

    public void EndInteraction()
    {
        _cardHolder.DOLocalMove(Vector3.zero, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
    }
}
