using System;
using UnityEngine;

public class PhaseSelectionState : InteractionStateBase
{
    public override string ConfigName => "0_PhaseSelection";
    public override string InteractionText => "Select two phases";
    public static event Action OnStart;
    public static event Action OnEnd;
    public static event Action OnConfirm;
    public static event Action OnReset;

    public override void EndState() => OnEnd?.Invoke();
    public override void StartState() => OnStart?.Invoke();
    public override void Initialize(CardPile[] piles) { 
        // TODO: Move this to a better place?
        numberSelections = 2;
    }

    public override void HandleConfirm(InteractionPanel ctx) => OnConfirm?.Invoke();
    public override void HandleSkip(InteractionPanel ctx) { }
    public override void HandleReset(InteractionPanel ctx) => OnReset?.Invoke();
}
