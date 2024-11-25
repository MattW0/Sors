using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

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
    public static event Action<TurnState, int, bool> OnInteractionBegin;

    private void Awake(){
        if (Instance == null) Instance = this;
        _selectionHandler = GetComponent<CardSelectionHandler>();
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel() => _selectionHandler.GetLocalPlayer();

    [TargetRpc]
    public void TargetStartInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"    - InteractionPanel: Choose {numberSelections} / {interactableCards.Count} cards");

        _state = turnState;
        _selectableCards = interactableCards;

        bool autoSkip = CheckAutoskip(numberSelections);

        OnInteractionBegin?.Invoke(turnState, numberSelections, autoSkip);
        if (autoSkip) return;

        // Make cards interactable
        if (_state == TurnState.Invent || _state == TurnState.Develop) MoneyCardsAreInteractable();
        else if (_state == TurnState.Recruit || _state == TurnState.Deploy) MoneyCardsAreInteractable();
        else AllCardsAreInteractable(true);
        
        // Move card collection
        if (_state == TurnState.CardSelection) _playerDiscard.StartInteraction();
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
        print("    - InteractionPanel: Selectable cards count: " + _selectableCards.Count);
        foreach(var card in _selectableCards) card.SetInteractable(b, _state);
    }

    private void MoneyCardsAreInteractable()
    {
        foreach (var card in _selectableCards) card.SetInteractable(card.cardInfo.type == CardType.Money, _state);
    }

    [ClientRpc]
    public void RpcResetPanel()
    {
        print("    - InteractionPanel: Reset panel");
        AllCardsAreInteractable(false);

        _playerHand.EndInteraction();
        _playerDiscard.EndInteraction();
        
        _selectionHandler.EndSelection();
    }

    [TargetRpc]
    public void TargetUndoMoneyPlay(NetworkConnection target) => MoneyCardsAreInteractable();
    public void SelectMarketTile(MarketTile tile) => _selectionHandler.SelectMarketTile(tile);
    public void DeselectMarketTile() => _selectionHandler.DeselectMarketTile();
    private bool ContainsMoney() => _selectableCards.Any(c => c.cardInfo.type == CardType.Money);
    private bool ContainsTechnology() => _selectableCards.Any(c => c.cardInfo.type == CardType.Technology);
    private bool ContainsCreature() => _selectableCards.Any(c => c.cardInfo.type == CardType.Creature);
}