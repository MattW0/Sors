using UnityEngine;
using System.Collections;

public class Life : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.Life);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.Life);

        target.EntityTakesDamage(- amount, false);
    }
}
