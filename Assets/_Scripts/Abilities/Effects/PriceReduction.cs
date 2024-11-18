using System.Collections;
using System;

public class PriceReduction : IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }
    public static event Action<PlayerManager, CardType, int> OnMarketPriceReduction;
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount)
    {
        VFXSystem.RpcPlayProjectile(source, target, Effect.PriceReduction);
        yield return VFXSystem.Wait();

        VFXSystem.RpcPlayHit(target, Effect.PriceReduction);
        var cardType = (CardType) Enum.Parse(typeof(CardType), target.cardType.ToString());
        OnMarketPriceReduction?.Invoke(source.Owner, cardType, amount);
    }
}
