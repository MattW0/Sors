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
    // Linking detail cards with their hand card gameobject
    private Dictionary<CardInfo, GameObject> _cache = new();
    private TurnState _state;
    private int _numberSelectableCards;
    public static event Action OnInteractionConfirmed;

    private void Awake(){
        if (Instance == null) Instance = this;
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

        // TODO: Implement and check if more interactions require other collection than hand
        if (turnState == TurnState.CardIntoHand) return;

        _playerHand.StartInteraction(turnState);
        _ui.InteractionBegin(turnState, numberSelectableCards);
    }

    #region States

    public void ConfirmSelection()
    {
        OnInteractionConfirmed?.Invoke();

        if (_state == TurnState.Discard) _player.CmdDiscardSelection(_selectedCards);
        else if (_state == TurnState.CardIntoHand || _state == TurnState.Trash) _player.CmdPrevailCardsSelection(_selectedCards);
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) _player.CmdPlayCard(_selectedCards[0]);
        // else if (_state == TurnState.Invent || _state == TurnState.Recruit) _player.CmdBuyCards(cards);
        
        _selectedCards.Clear();
    }

    public bool PlayerInteractionOnCard(GameObject card)
    {
        var cardStats = card.GetComponent<CardStats>();
        if (cardStats.IsSelected) {
            DeselectCard(card);
            return false;
        }

        // Remove the previously selected card if user clicks another one
        if (_selectedCards.Count >= _numberSelectableCards) 
            DeselectCard(_selectedCards.Last());
        
        SelectCard(card);
        return true;
    }

    #endregion

    private void SelectCard(GameObject card){
        _selectedCards.Add(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);

        card.GetComponent<CardStats>().IsSelected = true;
        _cardMover.MoveTo(card, true, CardLocation.Hand, CardLocation.Selection);
    }

    public void DeselectCard(GameObject card){
        _selectedCards.Remove(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);
        
        card.GetComponent<CardStats>().IsSelected = false;
        _cardMover.MoveTo(card, true, CardLocation.Selection, CardLocation.Hand);
    }

    public void SkipCardPlay() => _player.CmdSkipCardPlay();
    public void PlayerSkipsPrevailOption() => _player.CmdPlayerSkipsPrevailOption();

    [ClientRpc]
    public void RpcResetPanel(){
        ClearPanel();
        _ui.ResetPanelUI(true);
    }

    [ClientRpc]
    public void RpcSoftResetPanel(){
        ClearPanel();
        _ui.ResetPanelUI(false);
    }

    public void ClearPanel()
    {
        _playerHand.EndInteraction();

        _selectedCards.Clear();
        _cache.Clear();
    }
}

public enum InteractionType
{
    Play,
    Discard,
    Prevail
}
