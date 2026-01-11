using UnityEngine;

public class AttackState : NonCardInteractionState
{
    public override string ConfigName => "4_Attack";
    public override string InteractionText => "Choose Creatures to attack";

    public override void EndState() {}
    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCombatSelection();
    public override void HandleReset(InteractionPanel ctx) => ctx.ResetCombatArrows();
    public override void HandleSkip(InteractionPanel ctx) {}
}
