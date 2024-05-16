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
    private List<CardStats> _handCards = new();
    private TurnState _state;
    [SerializeField] private Transform _cardHolder;
    private Vector3 _handPositionPlayCards = new Vector3(-200, 100, 0);
    private Vector3 _handScalePlayCards = new Vector3(1.2f, 1.2f, 0);

    private void Awake()
    {
        if (!Instance) Instance = this;
        CardClick.OnCardClicked += ClickedCard;
    }

    private void Start()
    {
        _interactionPanel = InteractionPanel.Instance;
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) => HighlightCardTypes(isInteractable, new List<CardType> { CardType.Money });
    
    [TargetRpc]
    public void TargetHighlightMoney(NetworkConnection target) => HighlightCardTypes(true, new List<CardType> { CardType.Money });

    public void HighlightAllHandCards(bool b)
    {
        foreach(var card in _handCards) card.IsInteractable = b;
    }

    private void HighlightCardTypes(bool b, List<CardType> cardTypes)
    {
        foreach (var card in _handCards.Where(card => cardTypes.Contains(card.cardInfo.type))) card.IsInteractable = b;
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
        print("Starting interaction in state " + state);

        _state = state;
        _cardHolder.DOLocalMove(_handPositionPlayCards, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_handScalePlayCards, SorsTimings.cardPileRearrangement);
        
        if (state == TurnState.Discard) {
            HighlightAllHandCards(true);
            return;
        }

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

    private void ClickedCard(GameObject card)
    {
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        print($"Clicked card {cardInfo.title}");

        var b = _interactionPanel.PlayerInteractionOnCard(card);
    }

    private void OnDestroy()
    {
        CardClick.OnCardClicked -= ClickedCard;
    }
}
