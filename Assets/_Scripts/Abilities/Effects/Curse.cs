using System.Collections;
using System;

public class Curse : IEffect
{
    public static event Action<PlayerManager, int> OnPlayerGainsCurses;
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.Curse);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.Curse);
        OnPlayerGainsCurses?.Invoke(target.Owner, amount);
    }
}
