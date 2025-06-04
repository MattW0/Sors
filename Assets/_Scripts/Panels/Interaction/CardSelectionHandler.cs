using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(InteractionPanel))]
public class CardSelectionHandler : MonoBehaviour
{
    public List<CardStats> selectedCards = new();
    public MarketSelection? marketSelection;
    private InteractionPanel _interactionPanel;
    private InteractionUI _ui;
    private CardMover _cardMover;
    [SerializeField] private int _numberSelections;
    private CardInteractionState _state;
    public static event Action OnResetCards;

    private void Awake() 
    {
        _interactionPanel = gameObject.GetComponent<InteractionPanel>();
        _ui = gameObject.GetComponentInChildren<InteractionUI>();

        CardClickHandler.OnCardClicked += ClickedCard;
        MarketTile.OnTileSelected += SelectMarketTile;
        MarketTile.OnTileDeselected += DeselectMarketTile;
    }

    private void Start()
    {
        _cardMover = ServiceLocator.Global.Get<CardMover>();
    }

    public void BeginInteraction(CardInteractionState interactionState, int numberSelections)
    {
        _state = interactionState;
        _numberSelections = numberSelections;

        // Clear previous selections
        selectedCards.Clear();
        marketSelection = null;
    }

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();
        var destination = _state.GetCardDestination(cardStats) 
            ?? throw new Exception("Null exception on destination pile for state: " + _state.Config.turnState);

        // Debug.Log($"Clicked card {cardStats.cardInfo.title}, is selected: {cardStats.IsSelected}");
        
        // Check if player is playing money card
        if (destination == CardLocation.MoneyZone) _interactionPanel.LocalPlayer.Cards.PlayMoneyCard(cardStats);
        // Else we can select or deselect
        else if(cardStats.IsSelected) DeselectCard(cardStats);
        else SelectCard(cardStats);
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

    private void SelectMarketTile(MarketTile tile)
    {
        marketSelection = new MarketSelection(tile.cardInfo, tile.Cost, tile.Index);
        _ui.SelectMarketTile(tile.cardInfo);
    }

    private void DeselectMarketTile() => _ui.DeselectMarketTile();

    private void MoveCard(CardStats card, bool toSelection)
    {
        var pile = _state.Config.interactionPile;

        if(toSelection) {
            _cardMover.MoveTo(card.gameObject, true, pile, CardLocation.Selection);
            selectedCards.Add(card);
        } else {
            _cardMover.MoveTo(card.gameObject, true, CardLocation.Selection, pile);
            selectedCards.Remove(card);
        }

        _ui.SetConfirmButtonEnabled(_state.IsConfirmEnabled(selectedCards.Count()));
    }

    public void SkipCardInteraction()
    {
        // Need temp copy because MoveCard modifies selectedCards
        var tempList = new List<CardStats>(selectedCards);
        foreach (var card in tempList) MoveCard(card, false);
        selectedCards.Clear();
    }

    public void EndSelection()
    {
        _ui.PanelOut();
        OnResetCards?.Invoke();
    }

    private void OnDestroy()
    {
        CardClickHandler.OnCardClicked -= ClickedCard;
        MarketTile.OnTileSelected -= SelectMarketTile;
        MarketTile.OnTileDeselected -= DeselectMarketTile;
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