using UnityEngine;
using System.Collections;

public class CardDraw : IEffect
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
        _abilitiesVFXSystem.RpcPlayProjectile(source.transform.position, 
                                              target.transform.position, 
                                              Effect.CardDraw);
        yield return _wait;
        
        _abilitiesVFXSystem.RpcPlayHit(target.transform.position, Effect.MoneyGain);

        // TODO: Check if it be opponent?
        target.Owner.DrawCards(amount);
        yield return _wait;
    }
}
