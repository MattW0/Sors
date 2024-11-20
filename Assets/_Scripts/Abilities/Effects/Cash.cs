using System.Collections;

public class Cash : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.Cash);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.Cash);
        target.Owner.Cash += amount;
    }
}
