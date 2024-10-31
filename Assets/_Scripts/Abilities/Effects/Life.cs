using UnityEngine;
using System.Collections;

public class Life : IEffect
{
    private WaitForSeconds _wait = new WaitForSeconds(SorsTimings.effectProjectile);
    private AbilitiesVFXSystem _abilitiesVFXSystem;

    public void Init(AbilitiesVFXSystem abilitiesVFXSystem, WaitForSeconds wait)
    {
        _abilitiesVFXSystem = abilitiesVFXSystem;
        _wait = wait;
    }

    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        _abilitiesVFXSystem.RpcPlayProjectile(source, target, Effect.LifeGain);
        yield return _wait;

        _abilitiesVFXSystem.RpcPlayHit(target, Effect.LifeGain);

        // TODO: Lifegain for creatures and entities
        target.Owner.Health += amount;
    }
}
