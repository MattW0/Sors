using UnityEngine;
using System.Collections;

public class Damage : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        // Only play projectile if not targeting self
        if (source != target){
            VFXSystem.RpcPlayProjectile(source, target, Effect.Damage);
            yield return VFXSystem.Wait();
        }
        
        VFXSystem.RpcPlayHit(target, Effect.Damage);
        target.EntityTakesDamage(amount, source.CardInfo.traits.Contains(Traits.Deathtouch));
    }
}
