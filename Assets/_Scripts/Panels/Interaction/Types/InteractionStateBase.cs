using UnityEngine;
using System;


public abstract class InteractionStateBase : IInteractionState
{
    [Header("State Configuration")]
    public abstract string ConfigName { get; }
    public abstract string InteractionText { get; }
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
    public static event Action<InteractionType> OnConfirmInteraction;
    public static event Action<InteractionType> OnSkipInteraction;
    public static event Action OnResetInteraction;
    public abstract void Initialize(CardPile[] piles);
    public abstract void StartState();

    // Up-to vs exact interaction
    public virtual bool IsConfirmEnabled(int numberSelected)
    {
        if (Config.isUpTo) return numberSelected <= numberSelections;
        else return numberSelected == numberSelections;
    }

    public virtual void OnConfirm() => OnConfirmInteraction?.Invoke(Config.interactionType);
    public virtual void OnSkip() => OnSkipInteraction?.Invoke(Config.interactionType);
    public virtual void OnReset() => OnResetInteraction?.Invoke();
    public abstract void EndState();
}
