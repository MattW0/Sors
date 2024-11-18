using UnityEngine;
using System.Collections;

public class Life : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.LifeGain);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.LifeGain);

        // TODO: Lifegain for creatures and entities
        target.Owner.Health += amount;
    }
}
