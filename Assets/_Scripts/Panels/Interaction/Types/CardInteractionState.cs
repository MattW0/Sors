using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public abstract class CardInteractionState : InteractionStateBase
{
    public List<CardStats> selectableCards;
    public ICardPile InteractionPile { get; set; }
    public override void Initialize(CardPile[] piles)
    {
        InteractionPile = piles.FirstOrDefault(p => p.pileType == Config.interactionPile);

        if(InteractionPile == null) Debug.LogWarning("Interaction pile not set in InteractionPanel.cs or not defined for state config " + ConfigName);
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
    // default is to not skip a state
    public virtual bool CheckStateAutoskip() => false;

    // Makes some of the cards (eg. depending on type) interactable
    // Calls one of the Make{X}CardsInteractable functions
    public abstract void MakeCardsInteractable();

    // Defines where a card goes to when clicked (money cards during play/buy)
    // Returning null is equivalent to card is not clickable
    public abstract CardLocation? GetCardDestination(CardStats cardStats);

    // Only used for play interactions : develop, deploy
    protected virtual void CheckPlayability(int cash) 
    {
        // Since both states develop and deploy use this logic, for one of them selectableCards is null
        // Although valid only for develop and deploy, we have this here because CardInteractionState
        // tracks the selectableCards (and we can avoid that in InteractionPanel)
        if(selectableCards == null || Config.interactionType != InteractionType.Play) return;
        
        foreach (var card in selectableCards) {
            if (card.cardInfo.type != Config.cardType) continue;

            card.CheckPlayability(cash);
        }
    }
    protected bool SelectablesContainMoney() => selectableCards?.Any(c => c.cardInfo.type == CardType.Money) ?? false;
    protected bool SelectablesContainTechnology() => selectableCards?.Any(c => c.cardInfo.type == CardType.Technology) ?? false;
    protected bool SelectablesContainCreature() => selectableCards?.Any(c => c.cardInfo.type == CardType.Creature) ?? false;
    protected void MakeAllCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(true, Config.turnState));
    protected void MakeMoneyCardsInteractable()
        => selectableCards.ForEach(c => c.SetInteractable(c.cardInfo.type == CardType.Money, Config.turnState));
    
    public override void EndState() => InteractionPile.EndInteraction();
}
