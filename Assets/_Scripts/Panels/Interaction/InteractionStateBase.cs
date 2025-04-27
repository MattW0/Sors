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
    public InteractionPileUI InteractionPile { get; set; }

    [Header("Helper fields")]
    public int numberSelections;
    private List<CardStats> _selectableCards;
    public static event Action OnSkipInteraction;
    public static event Action OnResetInteraction;
    public static event Action<InteractionType> OnConfirmInteraction;
    public static event Action OnResetCards;

    public void Initialize(InteractionPileUI[] piles)
    {
        var path = "InteractionStateConfigs/" + ConfigName;
        Debug.Log($"Initializing interactions state {ConfigName}");

        config = Resources.Load<InteractionStateConfig>(path);
        if (config == null) Debug.LogWarning("Could not load interaction state config from resources: " + path);

        InteractionPile = piles.FirstOrDefault(p => p.Location == config.location);

        if(InteractionPile == null) Debug.LogWarning("Interaction pile not set in InteractionPanel.cs or not defined for state config " + ConfigName);
        else Debug.Log($"Interaction state {ConfigName} initialized");
    }

    public bool StartInteraction(List<CardStats> cards, int numberSelections)
    {
        // Returns true if auto-skip

        Debug.Log($"Start card interaction: {config.turnState}");
        _selectableCards = cards;
        this.numberSelections = numberSelections;

        // Auto-skip
        if (numberSelections <= 0) return true;
        if (cards.Count == 0) return true;
        if (CheckStateAutoskip()) return true;

        // Start interaction visuals
        InteractionPile.StartInteraction();
        MakeCardsInteractable(cards);

        return false;
    }

    internal void StartCombatInteraction(bool skip)
    {
        Debug.Log($"Start combat interaction: {config.turnState}");
    }

    public abstract void MakeCardsInteractable(List<CardStats> cards);

    // Virtual default implementations is for 'Discard' state
    // No auto-skip, select from all, exact number of selections 
    public virtual bool CheckStateAutoskip() => false;
    public virtual CardLocation? GetDestination(CardStats cardStats) => CardLocation.Selection;
    public virtual bool IsConfirmEnabled(int numberSelected) => numberSelected == numberSelections;
    public virtual void OnSkip() => OnSkipInteraction?.Invoke();
    public void OnReset() => OnResetInteraction?.Invoke();
    public virtual void OnConfirm() => OnConfirmInteraction?.Invoke(InteractionType.Select);
    public virtual void Reset()
    {
        OnResetCards?.Invoke();
        InteractionPile.EndInteraction();
    }
    
    protected bool ContainsMoney() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Money) ?? false;
    protected bool ContainsTechnology() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Technology) ?? false;
    protected bool ContainsCreature() => _selectableCards?.Any(c => c.cardInfo.type == CardType.Creature) ?? false;

    internal void CheckPlayability(CardType allowedType, int cash)
    {
        foreach (var card in _selectableCards) {
            if (card.cardInfo.type != allowedType) continue;

            card.CheckPlayability(cash);
        }
    }
}
