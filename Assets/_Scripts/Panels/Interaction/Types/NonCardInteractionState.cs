using UnityEngine;

public abstract class NonCardInteractionState : InteractionStateBase
{
    // Intentional no-op as its only used for card selections
    public override void Initialize(CardPile[] piles) => Debug.Log($"Interaction state {ConfigName} initialized");
    public override void StartState() {}
}
