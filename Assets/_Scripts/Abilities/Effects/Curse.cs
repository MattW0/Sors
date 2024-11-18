using System.Collections;
using System;
using Cysharp.Threading.Tasks.Triggers;

public class Curse : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public static event Action<PlayerManager, int> OnPlayerGainsCurse;
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.Curse);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.Curse);
        OnPlayerGainsCurse?.Invoke(target.Owner, amount);
    }
}
