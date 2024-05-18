using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    private Hand _playerHand;
    private CardMover _cardMover;
    private InteractionUI _ui;
    private PlayerManager _player;

    [Header("Helper Fields")]
    private List<GameObject> _selectedCards = new();
    private MarketSelection _marketSelection;
    private TurnState _state;
    private int _numberSelectableCards;
    public static event Action OnInteractionConfirmed;

    private void Awake(){
        if (Instance == null) Instance = this;
        CardClick.OnCardClicked += ClickedCard;
    }

    private void Start(){
        _playerHand = Hand.Instance;
        _cardMover = CardMover.Instance;
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel(int nbCardsToDiscard){
        _ui = gameObject.GetComponent<InteractionUI>();
        _ui.PrepareInteractionPanel(nbCardsToDiscard);
        _player = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetStartInteraction(NetworkConnection target, TurnState turnState, int numberSelectableCards)
    {
        _state = turnState;
        _numberSelectableCards = numberSelectableCards;
        bool autoSkip = CheckAutoskip();

        // TODO: Implement and check if more interactions require other collection than hand
        if (turnState == TurnState.CardIntoHand) return;

        _ui.InteractionBegin(turnState, autoSkip);
        if (!autoSkip) _playerHand.StartInteraction(turnState);
    }

    #region States

    public void ConfirmSelection()
    {
        OnInteractionConfirmed?.Invoke();

        if (_state == TurnState.Discard) {
            _playerHand.UpdateHandCards(_selectedCards, false);
            _player.CmdDiscardSelection(_selectedCards);
        }
        else if (_state == TurnState.CardIntoHand || _state == TurnState.Trash) _player.CmdPrevailCardsSelection(_selectedCards);
        else if (_state == TurnState.Invent || _state == TurnState.Recruit) _player.CmdConfirmBuy(_marketSelection);
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) {
            var card = _selectedCards[0];
            _playerHand.RemoveCard(card);
            _player.CmdConfirmPlay(card);
        }
        
        _selectedCards.Clear();
        _ui.ResetPanelUI(false);
    }

    private void ClickedCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();

        // Only select or deselect in these turnStates
        if (_state == TurnState.Discard || _state == TurnState.CardIntoHand || _state == TurnState.Trash) {
            SelectOrDeselectCard(card, cardStats.IsSelected);
            return;
        }

        // Have to check if playing money card
        if (cardStats.cardInfo.type == CardType.Money) {
            _player.CmdPlayMoneyCard(card, cardStats.cardInfo);
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
        if (isSelected) {
            DeselectCard(card);
            return;
        }

        // Remove the previously selected card if user clicks another one
        if (_selectedCards.Count >= _numberSelectableCards) 
            DeselectCard(_selectedCards.Last());
        
        SelectCard(card);
    }

    #endregion

    private void SelectCard(GameObject card)
    {
        _selectedCards.Add(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);

        card.GetComponent<CardStats>().IsSelected = true;
        _cardMover.MoveTo(card, true, CardLocation.Hand, CardLocation.Selection);
    }

    public void DeselectCard(GameObject card)
    {
        _selectedCards.Remove(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);
        
        card.GetComponent<CardStats>().IsSelected = false;
        _cardMover.MoveTo(card, true, CardLocation.Selection, CardLocation.Hand);
    }

    public void OnSkipInteraction() => _player.CmdSkipInteraction();

    private bool CheckAutoskip()
    {
        if (_numberSelectableCards <= 0) return true;

        // No card in hand -> only valid where player has to interact from hand
        if ((_state == TurnState.Discard || _state == TurnState.Trash) && _playerHand.HandCardsCount == 0) return true;

        // No entity to play
        if (_state == TurnState.Develop) return ! _playerHand.ContainsTechnology();
        else if (_state == TurnState.Deploy) return ! _playerHand.ContainsCreature();

        return false;
    }

    [ClientRpc]
    public void RpcResetPanel(){
        _playerHand.EndInteraction();
        _selectedCards.Clear();

        _ui.ResetPanelUI(true);
    }

    private void OnDestroy()
    {
        CardClick.OnCardClicked -= ClickedCard;
    }
}

public struct MarketSelection{
    public CardInfo cardInfo;
    public int cost;
    public int index;

    public MarketSelection(CardInfo cardInfo, int cost, int index){
        this.cardInfo = cardInfo;
        this.cost = cost;
        this.index = index;
    }
}
