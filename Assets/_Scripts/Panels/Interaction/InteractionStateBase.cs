using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// Each state is defined uniquely by the path to the SO state config
internal interface IInteractionState { 
    public string ConfigName { get; }
}

public abstract class InteractionStateBase : IInteractionState
{
    [Header("State Configuration")]
    public InteractionStateConfig config;
    public abstract string ConfigName { get; }
    public InteractionPileUI InteractionPile { get; set; }
    private List<CardStats> _selectableCards;
    public static event Action OnSkipInteraction;
    public static event Action OnResetInteraction;
    public static event Action OnConfirmInteraction;

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

    public void StartInteraction(List<CardStats> cards, int numberSelections)
    {
        Debug.Log($"Start card interaction: {config.turnState}");
        _selectableCards = cards;

        // Auto-skip
        if (numberSelections <= 0) return;
        if (cards.Count == 0) return;
        if (CheckStateAutoskip(numberSelections)) return;

        // Start interaction visuals
        InteractionPile.StartInteraction();
        MakeCardsInteractable(cards);
    }

    internal void StartCombatInteraction(bool skip)
    {
        Debug.Log($"Start combat interaction: {config.turnState}");
    }

    public abstract bool CheckStateAutoskip(int numberSelections);
    public abstract void MakeCardsInteractable(List<CardStats> cards);
    
    public virtual void OnSkip()
    {
        OnSkipInteraction?.Invoke();
    }

    public void OnReset() => OnResetInteraction?.Invoke();
    public virtual void OnConfirm()
    {
        OnConfirmInteraction?.Invoke();
    }

    public virtual void Reset()
    {
        foreach(var card in _selectableCards) card.SetInteractable(false);
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
