using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardSelectionHandler : MonoBehaviour
{
    public List<CardStats> selectedCards = new();
    public MarketSelection marketSelection;
    private CardMover _cardMover;
    private InteractionUI _ui;
    [SerializeField] private int _numberSelections;
    private InteractionStateBase _state;
    public static event Action<CardStats> OnPlayMoneyCard;

    private void Awake() 
    {
        CardClickHandler.OnCardClicked += ClickedCard;        
    }

    private void Start()
    {
        _cardMover = CardMover.Instance;
        _ui = gameObject.GetComponentInChildren<InteractionUI>();
    }

    public void BeginInteraction(InteractionStateBase interactionState, int numberSelections)
    {
        _state = interactionState;
        _numberSelections = numberSelections;
    }

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();

        var destination = _state.GetDestination(cardStats);
        if(destination == null) return;

        // Check if player is playing money card
        if (destination == CardLocation.MoneyZone) 
        {
            OnPlayMoneyCard?.Invoke(cardStats);
            cardStats.SetInteractable(false);
        } else {
            // Else we can select or deselect
            // Debug.Log($"Clicked card {cardStats.cardInfo.title}, is selected: {cardStats.IsSelected}");

            if(cardStats.IsSelected) DeselectCard(cardStats);
            else SelectCard(cardStats);
        }
    }

    private void SelectCard(CardStats card)
    {
        print($"Select card : {card.cardInfo.title}");
        // Remove the previously selected card if user clicks another one
        if (selectedCards.Count >= _numberSelections)
            DeselectCard(selectedCards.Last());

        card.IsSelected = true;
        MoveCard(card, true);
    }

    private void DeselectCard(CardStats card)
    {
        print($"Deselect card : {card.cardInfo.title}");

        card.IsSelected = false;
        MoveCard(card, false);
    }

    public void SelectMarketTile(MarketTile tile)
    {
        marketSelection = new MarketSelection(tile.cardInfo, tile.Cost, tile.Index);
        _ui.SelectMarketTile(tile.cardInfo);
    }

    public void DeselectMarketTile() => _ui.DeselectMarketTile();

    private void MoveCard(CardStats card, bool toSelection)
    {
        var pile = _state.config.location;

        if(toSelection) {
            _cardMover.MoveTo(card.gameObject, true, pile, CardLocation.Selection);
            selectedCards.Add(card);
        } else {
            _cardMover.MoveTo(card.gameObject, true, CardLocation.Selection, pile);
            selectedCards.Remove(card);
        }

        _ui.SetConfirmButtonEnabled(_state.IsConfirmEnabled(selectedCards.Count()));
    }

    public void EndSelection()
    {
        _ui.PanelOut();
        ClearSelection();
    }

    public void SkipCardInteraction()
    {
        ClearSelection();
    }

    private void ClearSelection()
    {
        // Need temp copy because MoveCard modifies selectedCards
        var tempList = new List<CardStats>(selectedCards);
        foreach (var card in tempList) MoveCard(card, false);
        selectedCards.Clear();
    }

    private void OnDestroy()
    {
        CardClickHandler.OnCardClicked -= ClickedCard;

    }
}

public struct MarketSelection
{
    public CardInfo cardInfo;
    public int cost;
    public int index;

    public MarketSelection(CardInfo cardInfo, int cost, int index){
        this.cardInfo = cardInfo;
        this.cost = cost;
        this.index = index;
    }
}