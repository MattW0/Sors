public abstract class NonCardInteractionState : InteractionStateBase
{
    // Intentional no-op as its only used for card selections
    public override void Initialize(CardPile[] piles) {}
    public override void StartState() {}
}
