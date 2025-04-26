using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// [CreateAssetMenu(fileName = "NewInteractionStateBase", menuName = "Sors/Interaction/InteractionStateBase")]
public abstract class InteractionStateBase : IInteractionState
{
    [Header("State Configuration")]
    public InteractionStateConfig config;
    private List<CardStats> _selectableCards;
    public abstract string ConfigName { get; }

    public static event Action OnSkipInteraction;
    public static event Action OnResetInteraction;
    public static event Action OnConfirmInteraction;

    public void Init() 
    {
        var path = "InteractionStateConfigs/" + ConfigName;

        config = Resources.Load<InteractionStateConfig>(path);
        if (config == null) Debug.LogWarning("Could not load interaction state config from resources: " + path);
        else Debug.Log("Initialized state: " + ConfigName);
    }

    internal virtual void StartInteraction(List<CardStats> cards, int numberSelections)
    {
        Debug.Log($"Start card interaction: {config.turnState}");

        _selectableCards = cards;
        bool autoSkip = CheckAutoskip(numberSelections);
    }

    internal void StartCombatInteraction(bool skip)
    {
        Debug.Log($"Start combat interaction: {config.turnState}");
    }

    private bool CheckAutoskip(int numberSelections)
    {
        // Nothing to select
        if (numberSelections <= 0) return true;
        if (_selectableCards.Count == 0) return true;

        return CheckStateSpecificAutoskip();
    }

    public abstract bool CheckStateSpecificAutoskip();

    // No entity to play
    // if (_state == TurnState.Develop) return ! ContainsTechnology();
    // if (_state == TurnState.Deploy) return ! ContainsCreature();base.CheckStateSpecificAutoskip

    public abstract void MakeCardsInteractable(List<CardStats> cards);
    // {
    //     foreach(var card in cards) card.SetInteractable(true, turnState);
    // }

    public void ResetCards()
    {
        foreach(var card in _selectableCards) card.SetInteractable(false);
    }

    // private void MoneyCardsAreInteractable()
    // {
    //     foreach (var card in _selectableCards) card.SetInteractable(card.cardInfo.type == CardType.Money, _state);
    // }

    
    public virtual void OnSkip()
    {
        OnSkipInteraction?.Invoke();
    }

    public void OnReset() => OnResetInteraction?.Invoke();
    public virtual void OnConfirm()
    {
        OnConfirmInteraction?.Invoke();
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

internal interface IInteractionState
{
    public string ConfigName { get; }
}