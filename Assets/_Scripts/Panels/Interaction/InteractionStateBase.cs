using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// Each state is defined uniquely by the path to the SO state config
public interface IInteractionState { 
    public string ConfigName { get; }
}

public abstract class InteractionStateBase : IInteractionState
{
    [Header("State Configuration")]
    public InteractionStateConfig config;
    public abstract string ConfigName { get; }
    public abstract string InteractionText { get; }
    public InteractionPileUI InteractionPile { get; set; }

    [Header("Helper fields")]
    public int numberSelections;
    private List<CardStats> _selectableCards;
    public static event Action OnSkipInteraction;
    public static event Action OnResetInteraction;
    public static event Action<InteractionType> OnConfirmInteraction;

    public void Initialize(InteractionPileUI[] piles)
    {
        var path = "InteractionStateConfigs/" + ConfigName;

        config = Resources.Load<InteractionStateConfig>(path);
        if (config == null) Debug.LogWarning("Could not load interaction state config from resources: " + path);

        InteractionPile = piles.FirstOrDefault(p => p.Location == config.interactionPile);

        if(InteractionPile == null) Debug.LogWarning("Interaction pile not set in InteractionPanel.cs or not defined for state config " + ConfigName);
        else Debug.Log($"Interaction state {ConfigName} initialized");
    }

    public bool StartInteraction(List<CardStats> cards, int numberSelections)
    {
        _selectableCards = cards;
        this.numberSelections = numberSelections;

        // Auto-skip
        if (numberSelections <= 0) return true;
        if (cards.Count == 0) return true;
        if (CheckStateAutoskip()) return true;

        // Start interaction visuals
        InteractionPile.StartInteraction();
        MakeCardsInteractable();

        return false;
    }

    internal void StartCombatInteraction(bool skip)
    {
        Debug.Log($"Start combat interaction: {config.turnState}");
    }


    // Returns bool given the available cards to select from
    // True: auto-skip and player is not able to give inputs
    public abstract bool CheckStateAutoskip();

    // Makes some of the cards (eg. depending on type) interactable
    // Calls one of the Make{X}CardsInteractable functions
    public abstract void MakeCardsInteractable();

    // Defines where a card goes to when clicked (money cards during play/buy)
    // Returning null is equivalent to card is not clickable
    public abstract CardLocation? GetCardDestination(CardStats cardStats);

    // Up-to vs exact interaction
    public virtual bool IsConfirmEnabled(int numberSelected)
    {
        if (config.isUpTo) return numberSelected <= numberSelections;
        else return numberSelected == numberSelections;
    }
    public virtual void OnConfirm() => OnConfirmInteraction?.Invoke(config.interactionType);
    public virtual void OnSkip() => OnSkipInteraction?.Invoke();
    public virtual void OnReset() => OnResetInteraction?.Invoke();
    public virtual void Reset() => InteractionPile.EndInteraction();
    
    protected bool ContainsMoney() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Money) ?? false;
    protected bool ContainsTechnology() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Technology) ?? false;
    protected bool ContainsCreature() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Creature) ?? false;
    protected void MakeAllCardsInteractable()
        => _selectableCards.ForEach(c => c.SetInteractable(true, config.turnState));
    
    protected void MakeMoneyCardsInteractable()
        => _selectableCards.ForEach(c => c.SetInteractable(c.cardInfo.type == CardType.Money, config.turnState));

    internal void CheckPlayability(CardType allowedType, int cash)
    {
        foreach (var card in _selectableCards) {
            if (card.cardInfo.type != allowedType) continue;

            card.CheckPlayability(cash);
        }
    }

        // if (! displayText.IsNullOrWhitespace()) return displayText;

        // string countText = isUpTo ? $"up to {nbCardsToSelectMax}" : nbCardsToSelectMax.ToString();
        // return string.Format(displayTextFormat, interactionType.ToString(), countText, FormatCardTypeText());
    // public abstract void FormatInteractionText();

    private string FormatCardTypeText()
    {
        // if (config.allowedCardType == CardType.All) return "";

        // // Return type to play (or buy) and in case of buying, add money 
        // return allowedCardType.ToString() + (interactionType == InteractionType.Buy ? " or Money " : " ");

        return "";
    }
}
