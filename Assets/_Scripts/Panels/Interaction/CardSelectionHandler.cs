using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

[RequireComponent(typeof(InteractionPanel))]
public class CardSelectionHandler : MonoBehaviour
{
    public Stack<CardStats> selectedCards = new();
    public CardSelection cardSelection;
    private InteractionPanel _interactionPanel;
    private InteractionUI _ui;
    [SerializeField] private int _numberSelections;
    private CardInteractionState _state;
    public static event Action OnResetCards;
    public static event Action<CardDragHandler, bool> OnCardSelection;

    private void Awake() 
    {
        _interactionPanel = gameObject.GetComponent<InteractionPanel>();
        _ui = gameObject.GetComponentInChildren<InteractionUI>();

        CardDragHandler.OnCardClicked += ClickedCard;
        MarketTile.OnTileSelected += SelectMarketTile;
        MarketTile.OnTileDeselected += DeselectMarketTile;
    }

    public void BeginInteraction(CardInteractionState interactionState, int numberSelections)
    {
        _state = interactionState;
        _numberSelections = numberSelections;

        // Clear previous selections
        selectedCards.Clear();
        cardSelection.Clear();
    }

    private void ClickedCard(CardStats cardStats)
    {
        var destination = _state.GetCardDestination(cardStats) 
            ?? throw new Exception("Null exception on destination pile for state: " + _state.Config.turnState);
        
        // Check if player is playing money card
        if (destination == CardLocation.MoneyZone) {
            OnCardSelection?.Invoke(cardStats.DragHandler, true);
            _interactionPanel.LocalPlayer.Cards.PlayMoneyCard(cardStats);
            return;
        }

        // Else we can select or deselect
        print($"Number of selected cards: {selectedCards.Count()}");
        
        if(selectedCards.Contains(cardStats)) DeselectCard();
        else SelectCard(cardStats).Forget();
    }

    private async UniTaskVoid SelectCard(CardStats card)
    {
        // Remove the previously selected card if user clicks another one
        if (selectedCards.Count >= _numberSelections)
            DeselectCard();
            await UniTask.Delay(millisecondsDelay: SorsTimings.waitShort);

        cardSelection = new CardSelection(card.cardInfo, card.cardInfo.cost);
        
        OnCardSelection?.Invoke(card.DragHandler, true);
        selectedCards.Push(card);

        _ui.SetConfirmButtonEnabled(_state.IsConfirmEnabled(selectedCards.Count()));
    }

    private void DeselectCard()
    {
        OnCardSelection?.Invoke(selectedCards.Pop().DragHandler, false);
    }

    private void SelectMarketTile(MarketTile tile)
    {
        cardSelection = new CardSelection(tile.cardInfo, tile.Cost, tile.Index);
        _ui.SelectMarketTile(tile.cardInfo);
    }

    private void DeselectMarketTile() => _ui.DeselectMarketTile();

    public void SkipCardInteraction()
    {
        EmptySelectionStack();

        _ui.SetConfirmButtonEnabled(_state.IsConfirmEnabled(selectedCards.Count()));
    }

    internal void UndoMoneyPlay(InteractionType type)
    {
        if (type == InteractionType.Buy) {
            cardSelection.Clear();
            _ui.DeselectMarketTile();
        } else {
            EmptySelectionStack();
        }

        var undoables = _interactionPanel.LocalPlayer.Cards.UndoPlayMoney();
        foreach (var card in undoables) OnCardSelection?.Invoke(card.DragHandler, false);
    }

    private void EmptySelectionStack()
    {
        while (selectedCards.Count > 0)
            OnCardSelection?.Invoke(selectedCards.Pop().DragHandler, false);
    }

    public void EndSelection()
    {
        _ui.PanelOut();
        OnResetCards?.Invoke();
    }

    private void OnDestroy()
    {
        CardDragHandler.OnCardClicked -= ClickedCard;
        MarketTile.OnTileSelected -= SelectMarketTile;
        MarketTile.OnTileDeselected -= DeselectMarketTile;
    }
}

public struct CardSelection
{
    public CardInfo? cardInfo;
    public int cost;
    public int marketIndex;

    public CardSelection(CardInfo card, int cost, int index = -1){
        this.cardInfo = card;
        this.cost = cost;
        this.marketIndex = index;
    }

    public void Clear() {  cardInfo = null; }
}