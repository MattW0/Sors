using UnityEngine;

public class BlockState : NonCardInteractionState
{
    public override string ConfigName => "5_Block";
    public override string InteractionText => "Choose Creatures to block";
    public override void EndState() {}
    public override void HandleConfirm(InteractionPanel ctx) => ctx.ConfirmCombatSelection();
    public override void HandleReset(InteractionPanel ctx) => ctx.ResetCombatArrows();
    public override void HandleSkip(InteractionPanel ctx) {}
}
