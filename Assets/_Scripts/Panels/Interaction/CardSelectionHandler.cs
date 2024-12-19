using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardSelectionHandler : MonoBehaviour
{
    private PlayerManager _player;
    private List<CardStats> _selectedCards = new();
    private CardMover _cardMover;
    private InteractionUI _ui;
    [SerializeField] private int _numberSelections;
    private MarketSelection _marketSelection;
    private TurnState _state;
    public static event Action OnInteractionConfirmed;

    private void Start()
    {
        _cardMover = CardMover.Instance;
        _ui = gameObject.GetComponentInChildren<InteractionUI>();

        CardClickHandler.OnCardClicked += ClickedCard;
        InteractionPanel.OnInteractionBegin += BeginInteraction;
    }

    public void GetLocalPlayer() => _player = PlayerManager.GetLocalPlayer();
    public void BeginInteraction(TurnState turnState, int numberSelections, bool autoSkip)
    {
        _state = turnState;
        _numberSelections = numberSelections;

        if (_state == TurnState.Trash || _state == TurnState.CardSelection) _ui.SetConfirmButtonEnabled(true);
    }

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();
        print("Clicked " + card.name + " in state " + _state);

        // Only select or deselect in these turnStates (all card types behave the same way)
        if (_state == TurnState.Discard || _state == TurnState.CardSelection || _state == TurnState.Trash) {
            SelectOrDeselectCard(cardStats);
            return;
        }

        // Have to check if playing money card
        if (cardStats.cardInfo.type == CardType.Money) {
            _player.Cards.CmdPlayMoneyCard(cardStats);
            cardStats.SetInteractable(false);
            return;
        }

        // Else we can select or deselect entity card
        SelectOrDeselectCard(cardStats);
    }

    private void SelectOrDeselectCard(CardStats card)
    {
        print("Selecting or deselecting card, isSelected: " + card.IsSelected);
        if (card.IsSelected) DeselectCard(card);
        else SelectCard(card);
    }

    private void SelectCard(CardStats card)
    {
        // Remove the previously selected card if user clicks another one
        if (_selectedCards.Count >= _numberSelections)
            DeselectCard(_selectedCards.Last());

        card.IsSelected = true;
        MoveCard(card, true);
        CheckConfirmButtonState();
    }

    private void DeselectCard(CardStats card)
    {
        card.IsSelected = false;
        MoveCard(card, false);
        CheckConfirmButtonState();
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
        else if (_state == TurnState.CardSelection || _state == TurnState.Trash) _player.CmdPrevailCardsSelection(_selectedCards);
        else if (_state == TurnState.Invent || _state == TurnState.Recruit) _player.CmdConfirmBuy(_marketSelection);
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) _player.CmdConfirmPlay(_selectedCards);
        
        _selectedCards.Clear();
    }

    private void MoveCard(CardStats card, bool toSelection)
    {
        var pile = _state switch
        {
            TurnState.CardSelection => CardLocation.Discard,
            _ => CardLocation.Hand
        };

        if(toSelection) {
            _cardMover.MoveTo(card.gameObject, true, pile, CardLocation.Selection);
            _selectedCards.Add(card);
        } else {
            _cardMover.MoveTo(card.gameObject, true, CardLocation.Selection, pile);
            _selectedCards.Remove(card);
        }
    }

    private void CheckConfirmButtonState()
    {
        // UP TO selection: All states where the number selections <= X
        if (_state == TurnState.Trash || _state == TurnState.CardSelection) return;

        // Otherwise enable confirm button only if number selections = X
        _ui.SetConfirmButtonEnabled(_selectedCards.Count == _numberSelections);
    }

    public void EndSelection()
    {
        _ui.PanelOut();
        ClearSelection();
    }

    public void SkipInteraction()
    {
        _player.CmdSkipInteraction();
        ClearSelection();
    }

    private void ClearSelection()
    {
        var tempList = new List<CardStats>(_selectedCards);
        foreach (var card in tempList) MoveCard(card, false);
        _selectedCards.Clear();
    }

    private void OnDestroy()
    {
        CardClickHandler.OnCardClicked -= ClickedCard;
        InteractionPanel.OnInteractionBegin -= BeginInteraction;
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