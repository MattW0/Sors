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
    public abstract void Initialize(CardPile[] piles);
    public abstract void StartState();
    public abstract void HandleConfirm(InteractionPanel ctx);
    public abstract void HandleSkip(InteractionPanel ctx);
    public abstract void HandleReset(InteractionPanel ctx);

    // Up-to vs exact interaction
    public virtual bool IsConfirmEnabled(int numberSelected)
    {
        if (Config.isUpTo) return numberSelected <= numberSelections;
        else return numberSelected == numberSelections;
    }
    public abstract void EndState();
}
