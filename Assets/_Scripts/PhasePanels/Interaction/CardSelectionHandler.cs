using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardSelectionHandler : MonoBehaviour
{
    private PlayerManager _player;
    private List<GameObject> _selectedCards = new();
    private CardMover _cardMover;
    private InteractionUI _ui;
    [SerializeField] private int _numberSelections;
    [SerializeField] private int _numberSelected;
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

        MoveCard(card, true);

        if (_selectedCards.Count == _numberSelections)
            _ui.EnableConfirmButton(true);
    }

    private void DeselectCard(GameObject card)
    {
        MoveCard(card, false);

        if (_state == TurnState.Trash || _state == TurnState.CardIntoHand) _ui.EnableConfirmButton(true);
        else if (_selectedCards.Count < _numberSelections) _ui.EnableConfirmButton(false);
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
        // _ui.ResetPanelUI(false);
    }

    private void MoveCard(GameObject card, bool toSelection)
    {
        card.GetComponent<CardStats>().IsSelected = toSelection;

        var pile = _state switch
        {
            TurnState.CardIntoHand => CardLocation.Discard,
            _ => CardLocation.Hand
        };

        if(toSelection) {
            _cardMover.MoveTo(card, true, pile, CardLocation.Selection);
            _selectedCards.Add(card);
        } else {
            _cardMover.MoveTo(card, true, CardLocation.Selection, pile);
            _selectedCards.Remove(card);
        }

        _numberSelected = _selectedCards.Count;
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