using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    [SerializeField] private CardPileInteraction _playerHand;
    [SerializeField] private CardPileInteraction _playerDiscard;
    private CardMover _cardMover;
    private InteractionUI _ui;
    private PlayerManager _player;

    [Header("Helper Fields")]
    [SerializeField] private List<CardStats> _selectableCards = new();
    private List<GameObject> _selectedCards = new();
    private MarketSelection _marketSelection;
    private TurnState _state;
    private int _numberSelections;
    public static event Action OnInteractionConfirmed;

    private void Awake(){
        if (Instance == null) Instance = this;
        CardClick.OnCardClicked += ClickedCard;
    }

    private void Start()
    {
        _ui = gameObject.GetComponent<InteractionUI>();
        _cardMover = CardMover.Instance;
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel() => _player = PlayerManager.GetLocalPlayer();

    [TargetRpc]
    public void TargetStartInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"Start interaction in state {turnState} choose {numberSelections} / {interactableCards.Count} selectable cards");

        _state = turnState;
        _numberSelections = numberSelections;
        _selectableCards = interactableCards;

        bool autoSkip = CheckAutoskip();
        _ui.InteractionBegin(turnState, autoSkip, numberSelections);
        if (autoSkip) return;

        // All cards are interactable
        if (_state == TurnState.Discard 
            || _state == TurnState.Trash 
            || _state == TurnState.CardIntoHand) 
            AllCardsAreInteractable(true);
        else 
            MoneyCardsAreInteractable();

        if (_state == TurnState.CardIntoHand) _playerDiscard.StartInteraction();
        else _playerHand.StartInteraction();
    }

    #region States

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, TurnState state, int newAmount)
    {
        var allowedType = state switch
        {
            TurnState.Develop => CardType.Technology,
            TurnState.Deploy => CardType.Creature,
            _ => CardType.None
        };

        foreach (var card in _selectableCards) {
            if (card.cardInfo.type != allowedType) continue;

            card.CheckPlayability(newAmount);
        }
    }

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

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();

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

    public void SelectMarketTile(MarketTile tile)
    {
        _marketSelection = new MarketSelection(tile.cardInfo, tile.Cost, tile.Index);
        _ui.SelectMarketTile(tile.cardInfo);
    }

    public void DeselectMarketTile() => _ui.DeselectMarketTile();

    private void SelectOrDeselectCard(GameObject card, bool isSelected)
    {
        if (isSelected) DeselectCard(card);
        else SelectCard(card);
    }

    #endregion

    private void SelectCard(GameObject card)
    {
        // Remove the previously selected card if user clicks another one
        if (_selectedCards.Count >= _numberSelections) 
            DeselectCard(_selectedCards.Last());

        _selectedCards.Add(card);
        card.GetComponent<CardStats>().IsSelected = true;
        _cardMover.MoveTo(card, true, CardLocation.Hand, CardLocation.Selection);

        if (_selectedCards.Count >= _numberSelections) 
            _ui.EnableConfirmButton(true);
    }

    public void DeselectCard(GameObject card)
    {
        _selectedCards.Remove(card);        
        card.GetComponent<CardStats>().IsSelected = false;

        _cardMover.MoveTo(card, true, CardLocation.Selection, CardLocation.Hand);
        _ui.EnableConfirmButton(false);
    }

    private void AllCardsAreInteractable(bool b)
    {
        foreach(var card in _selectableCards) card.IsInteractable = b;
    }

    private void MoneyCardsAreInteractable()
    {
        foreach(var card in _selectableCards) if (card.cardInfo.type == CardType.Money) card.IsInteractable = true;
    }

    [TargetRpc]
    // Only used for undo on playing money cards
    public void TargetUndoMoneyPlay(NetworkConnection target) => MoneyCardsAreInteractable();

    public void OnSkipInteraction() => _player.CmdSkipInteraction();

    private bool CheckAutoskip()
    {
        // Nothing to select
        if (_numberSelections <= 0) return true;
        if (_selectableCards.Count == 0) return true;

        // No entity to play
        if (_state == TurnState.Develop) return ! ContainsTechnology();
        if (_state == TurnState.Deploy) return ! ContainsCreature();

        return false;
    }

    private bool ContainsMoney() => _selectableCards.Any(c => c.cardInfo.type == CardType.Money);
    private bool ContainsTechnology() => _selectableCards.Any(c => c.cardInfo.type == CardType.Technology);
    private bool ContainsCreature() => _selectableCards.Any(c => c.cardInfo.type == CardType.Creature);

    [ClientRpc]
    public void RpcResetPanel()
    {
        AllCardsAreInteractable(false);
        _playerHand.EndInteraction();
        _selectedCards.Clear();

        _ui.ResetPanelUI(true);
    }

    private void OnDestroy()
    {
        CardClick.OnCardClicked -= ClickedCard;
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