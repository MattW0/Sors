using UnityEngine;
using System.Collections;
using System;

public class PriceReduction : IEffect
{
    private WaitForSeconds _wait = new WaitForSeconds(SorsTimings.effectProjectile);
    private AbilitiesVFXSystem _abilitiesVFXSystem;
    public static event Action<PlayerManager, CardType, int> OnMarketPriceReduction;

    public void Init(AbilitiesVFXSystem abilitiesVFXSystem, WaitForSeconds wait)
    {
        _abilitiesVFXSystem = abilitiesVFXSystem;
        _wait = wait;
    }

    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        _abilitiesVFXSystem.RpcPlayProjectile(source, target, Effect.PriceReduction);
        yield return _wait;

        _abilitiesVFXSystem.RpcPlayHit(target, Effect.PriceReduction);
        var cardType = (CardType) Enum.Parse(typeof(CardType), target.cardType.ToString());
        OnMarketPriceReduction?.Invoke(source.Owner, cardType, amount);
    }
}
