using UnityEngine;

public class DamageState : NonCardInteractionState
{
    public override string ConfigName => "6_Damage";
    public override string InteractionText => "Damage is resolving";
    public override void EndState() {}
    public override void HandleConfirm(InteractionPanel ctx) {}
    public override void HandleReset(InteractionPanel ctx) {}
    public override void HandleSkip(InteractionPanel ctx) {}
}
