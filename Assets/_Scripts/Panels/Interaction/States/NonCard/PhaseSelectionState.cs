using System;
using UnityEngine;

public class PhaseSelectionState : InteractionStateBase
{
    public override string ConfigName => "0_PhaseSelection";
    public override string InteractionText => "Select two phases";
    public static event Action OnStart;
    public static event Action OnEnd;

    public override void EndState() => OnEnd?.Invoke();
    public override void StartState() => OnStart?.Invoke();
    public override void Initialize(CardPile[] piles) { }

}
