using UnityEngine;
using System.Collections;

public class CardDraw : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }

    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.CardDraw);
        yield return VFXSystem.Wait();
        
        VFXSystem.RpcPlayHit(target, Effect.MoneyGain);

        // TODO: Check if it be opponent?
        target.Owner.DrawCards(amount);
        yield return VFXSystem.Wait();
    }
}