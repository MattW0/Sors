using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class CardInteractionState : InteractionStateBase
{
    public List<CardStats> selectableCards;
    public InteractionPileUI InteractionPile { get; set; }
    public override void Initialize(InteractionPileUI[] piles)
    {
        InteractionPile = piles.FirstOrDefault(p => p.Location == Config.interactionPile);

        if(InteractionPile == null) Debug.LogWarning("Interaction pile not set in InteractionPanel.cs or not defined for state config " + ConfigName);
        else Debug.Log($"Interaction state {ConfigName} initialized");
    }

    public void InitializeInteraction(List<CardStats> cards, int numberSelections)
    {
        selectableCards = cards;
        this.numberSelections = numberSelections;
    }

    public bool CheckAutoskip()
    {
        // Auto-skip
        if (numberSelections <= 0) return true;
        if (selectableCards.Count == 0) return true;
        if (CheckStateAutoskip()) return true;

        return false;
    }

    public override void StartState()
    {
        // Start interaction visuals
        InteractionPile.StartInteraction();
        MakeCardsInteractable();
    }

    // Returns bool depending on the available cards to select from
    // True: auto-skip and player is not able to give inputs
    public abstract bool CheckStateAutoskip();

    // Makes some of the cards (eg. depending on type) interactable
    // Calls one of the Make{X}CardsInteractable functions
    public abstract void MakeCardsInteractable();

    // Defines where a card goes to when clicked (money cards during play/buy)
    // Returning null is equivalent to card is not clickable
    public abstract CardLocation? GetCardDestination(CardStats cardStats);

    // Only used for play interactions : develop, deploy
    internal virtual void CheckPlayability(int cash) { }
    public override void EndState() => InteractionPile.EndInteraction();

    protected bool ContainsMoney() => selectableCards?.Any(c => c.cardInfo.type == CardType.Money) ?? false;
    protected bool ContainsTechnology() => selectableCards?.Any(c => c.cardInfo.type == CardType.Technology) ?? false;
    protected bool ContainsCreature() => selectableCards?.Any(c => c.cardInfo.type == CardType.Creature) ?? false;
    protected void MakeAllCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(true, Config.turnState));
    
    protected void MakeMoneyCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(c.cardInfo.type == CardType.Money, Config.turnState));
}
