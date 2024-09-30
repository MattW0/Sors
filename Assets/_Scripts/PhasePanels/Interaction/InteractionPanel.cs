using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

[RequireComponent(typeof(CardSelectionHandler), typeof(CardSelectionHandler))]
public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    [SerializeField] private CardPileInteraction _playerHand;
    [SerializeField] private CardPileInteraction _playerDiscard;
    private CardSelectionHandler _selectionHandler; 

    [Header("Helper Fields")]
    [SerializeField] private List<CardStats> _selectableCards = new();
    private TurnState _state;

    private void Awake(){
        if (Instance == null) Instance = this;
        _selectionHandler = GetComponent<CardSelectionHandler>();
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel() => _selectionHandler.GetLocalPlayer();

    [TargetRpc]
    public void TargetStartInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"{turnState} interaction: Choose {numberSelections} out of {interactableCards.Count} selectable cards");

        _state = turnState;
        _selectableCards = interactableCards;

        bool autoSkip = CheckAutoskip(numberSelections);
        _selectionHandler.BeginInteraction(turnState, numberSelections, autoSkip);
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

    private bool CheckAutoskip(int numberSelections)
    {
        // Nothing to select
        if (numberSelections <= 0) return true;
        if (_selectableCards.Count == 0) return true;

        // No entity to play
        if (_state == TurnState.Develop) return ! ContainsTechnology();
        if (_state == TurnState.Deploy) return ! ContainsCreature();

        return false;
    }

    private void AllCardsAreInteractable(bool b)
    {
        foreach(var card in _selectableCards) 
            card.IsInteractable = b;
    }

    private void MoneyCardsAreInteractable()
    {
        foreach(var card in _selectableCards) 
            if (card.cardInfo.type == CardType.Money) 
                card.IsInteractable = true;
    }

    [TargetRpc]
    // Only used for undo on playing money cards
    public void TargetUndoMoneyPlay(NetworkConnection target) => MoneyCardsAreInteractable();
    
    public void SelectMarketTile(MarketTile tile) => _selectionHandler.SelectMarketTile(tile);
    public void DeselectMarketTile() => _selectionHandler.DeselectMarketTile();
    private bool ContainsMoney() => _selectableCards.Any(c => c.cardInfo.type == CardType.Money);
    private bool ContainsTechnology() => _selectableCards.Any(c => c.cardInfo.type == CardType.Technology);
    private bool ContainsCreature() => _selectableCards.Any(c => c.cardInfo.type == CardType.Creature);

    [ClientRpc]
    public void RpcResetPanel()
    {
        AllCardsAreInteractable(false);
        _playerHand.EndInteraction();
        _selectionHandler.EndSelection();
    }
}