using System.Collections;

public class Money : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.MoneyGain);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.MoneyGain);
        target.Owner.Cash += amount;
    }
}