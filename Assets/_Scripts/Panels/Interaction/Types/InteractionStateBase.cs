using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class InteractionStateBase : IInteractionState
{
    [Header("State Configuration")]
    public abstract string ConfigName { get; }
    public abstract string InteractionText { get; }
    public InteractionPileUI InteractionPile { get; set; }

    private InteractionStateConfig _config;
    public InteractionStateConfig Config
    {
        get {
            if(_config == null) {
                var path = "InteractionStateConfigs/" + ConfigName;
                _config = Resources.Load<InteractionStateConfig>(path);

                if (_config == null) Debug.LogWarning("Could not load interaction state config from " + path);
            }

            return _config;
        }
    }


    [Header("Helper fields")]
    public int numberSelections;
    public List<CardStats> selectableCards;
    public static event Action OnSkipInteraction;
    public static event Action OnResetInteraction;
    public static event Action<InteractionType> OnConfirmInteraction;

    // Intentional No-op
    // Only used for interactions without cards (phases, combat, ...) 
    public void Initialize() => Debug.LogWarning("Should not call Initialize with no arguments on card interaction state");

    public void Initialize(InteractionPileUI[] piles)
    {
        InteractionPile = piles.FirstOrDefault(p => p.Location == Config.interactionPile);

        if(InteractionPile == null) Debug.LogWarning("Interaction pile not set in InteractionPanel.cs or not defined for state config " + ConfigName);
        else Debug.Log($"Interaction state {ConfigName} initialized");
    }

    public bool StartInteraction(List<CardStats> cards, int numberSelections)
    {
        selectableCards = cards;
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
        Debug.Log($"Start combat interaction: {Config.turnState}");
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
        if (Config.isUpTo) return numberSelected <= numberSelections;
        else return numberSelected == numberSelections;
    }

    // Only used for play interactions : develop, deploy
    internal virtual void CheckPlayability(int cash) { }
    public virtual void OnConfirm() => OnConfirmInteraction?.Invoke(Config.interactionType);
    public virtual void OnSkip() => OnSkipInteraction?.Invoke();
    public virtual void OnReset() => OnResetInteraction?.Invoke();
    public virtual void EndState() => InteractionPile.EndInteraction();
    
    protected bool ContainsMoney() => selectableCards?.Any(c => c.cardInfo.type == CardType.Money) ?? false;
    protected bool ContainsTechnology() => selectableCards?.Any(c => c.cardInfo.type == CardType.Technology) ?? false;
    protected bool ContainsCreature() => selectableCards?.Any(c => c.cardInfo.type == CardType.Creature) ?? false;
    protected void MakeAllCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(true, Config.turnState));
    
    protected void MakeMoneyCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(c.cardInfo.type == CardType.Money, Config.turnState));
}
