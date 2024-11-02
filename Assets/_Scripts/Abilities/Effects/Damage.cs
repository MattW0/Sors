using UnityEngine;
using System.Collections;

public class Damage : IEffect
{
    private WaitForSeconds _wait;
    private AbilitiesVFXSystem _abilitiesVFXSystem;

    public void Init(AbilitiesVFXSystem abilitiesVFXSystem, WaitForSeconds wait)
    {
        _abilitiesVFXSystem = abilitiesVFXSystem;
        _wait = wait;
    }

    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        // Only play projectile if not targeting self
        if (source != target){
            _abilitiesVFXSystem.RpcPlayProjectile(source.transform.position, 
                                                  target.transform.position, 
                                                  Effect.Damage);
            yield return _wait;
        }
        
        _abilitiesVFXSystem.RpcPlayHit(target.transform.position, Effect.Damage);
        target.EntityTakesDamage(amount, source.CardInfo.traits.Contains(Traits.Deathtouch));
    }
}
