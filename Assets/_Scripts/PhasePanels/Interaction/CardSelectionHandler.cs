using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(InteractionUI))]
public class CardSelectionHandler : MonoBehaviour
{
    private PlayerManager _player;
    private List<GameObject> _selectedCards = new();
    private CardMover _cardMover;
    private InteractionUI _ui;
    private int _numberSelections;
    private bool _isUpToNumber;
    private MarketSelection _marketSelection;
    private TurnState _state;
    public static event Action OnInteractionConfirmed;

    private void Start()
    {
        _cardMover = CardMover.Instance;
        _ui = gameObject.GetComponent<InteractionUI>();

        CardClickHandler.OnCardClicked += ClickedCard;
    }

    public void GetLocalPlayer() => _player = PlayerManager.GetLocalPlayer();
    public void BeginInteraction(TurnState turnState, int numberSelections, bool autoSkip, bool isUpToNumber = false)
    {
        _state = turnState;
        _numberSelections = numberSelections;
        _isUpToNumber = isUpToNumber;
        _ui.InteractionBegin(turnState, autoSkip, numberSelections);
    }

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();
        // string selection = "Current Selection: ";
        // foreach(var selectedCard in _selectedCards) {
        //     selection += $"{selectedCard.GetComponent<CardStats>().cardInfo.title}, ";
        // }
        // print(selection);

        // Only select or deselect in these turnStates (all card types behave the same way)
        if (_state == TurnState.Discard || _state == TurnState.CardIntoHand || _state == TurnState.Trash) {
            SelectOrDeselectCard(card, cardStats.IsSelected);
            return;
        }

        // Have to check if playing money card
        if (cardStats.cardInfo.type == CardType.Money) {
            _player.CmdPlayMoneyCard(card, cardStats);
            cardStats.IsInteractable = false;
            return;
        }

        // Else we can select or deselect entity card
        SelectOrDeselectCard(card, cardStats.IsSelected);
    }

    private void SelectOrDeselectCard(GameObject card, bool isSelected)
    {
        if (isSelected) DeselectCard(card);
        else SelectCard(card);
    }

    private void SelectCard(GameObject card)
    {
        // Remove the previously selected card if user clicks another one
        if (_selectedCards.Count >= _numberSelections) 
            DeselectCard(_selectedCards.Last());

        _selectedCards.Add(card);
        card.GetComponent<CardStats>().IsSelected = true;

        var sourcePile = _state switch {
            TurnState.CardIntoHand => CardLocation.Discard,
            _ => CardLocation.Hand
        };
        _cardMover.MoveTo(card, true, sourcePile, CardLocation.Selection);

        if (_selectedCards.Count >= _numberSelections) 
            _ui.EnableConfirmButton(true);
    }

    public void DeselectCard(GameObject card)
    {
        _selectedCards.Remove(card);
        card.GetComponent<CardStats>().IsSelected = false;

        var destinationPile = _state switch {
            TurnState.CardIntoHand => CardLocation.Discard,
            _ => CardLocation.Hand
        };
        _cardMover.MoveTo(card, true, CardLocation.Selection, destinationPile);

        if (_selectedCards.Count < _numberSelections)
        _ui.EnableConfirmButton(false);
    }

    public void SelectMarketTile(MarketTile tile)
    {
        _marketSelection = new MarketSelection(tile.cardInfo, tile.Cost, tile.Index);
        _ui.SelectMarketTile(tile.cardInfo);
    }

    public void DeselectMarketTile() => _ui.DeselectMarketTile();

    public void ConfirmSelection()
    {
        OnInteractionConfirmed?.Invoke();

        if (_state == TurnState.Discard) _player.CmdDiscardSelection(_selectedCards);
        else if (_state == TurnState.CardIntoHand || _state == TurnState.Trash) _player.CmdPrevailCardsSelection(_selectedCards);
        else if (_state == TurnState.Invent || _state == TurnState.Recruit) _player.CmdConfirmBuy(_marketSelection);
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) _player.CmdConfirmPlay(_selectedCards[0]);
        
        _selectedCards.Clear();
        _ui.ResetPanelUI(false);
    }

    public void EndSelection()
    {
        foreach(var card in _selectedCards)
            DeselectCard(card);
        
        _selectedCards.Clear();
        _ui.ResetPanelUI(true);
    }

    public void OnSkipInteraction() => _player.CmdSkipInteraction();

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