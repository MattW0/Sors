using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EffectHandler : MonoBehaviour
{
    private TurnManager _turnManager;
    private AbilitiesVFXSystem _abilitiesVFXSystem;
    private Ability _ability;
    private BattleZoneEntity _abilitySource;
    // TODO: Make this a list for multiple targets
    private BattleZoneEntity _abilityTarget;

    private void Start()
    {
        _turnManager = TurnManager.Instance;
        _abilitiesVFXSystem = AbilitiesVFXSystem.Instance;
    }

    internal IEnumerator Execute()
    {
        // Sanity check
        if (_abilitySource == null) yield break;

        if (_ability.effect == Effect.Damage) yield return HandleDamage();
        else if (_ability.effect == Effect.LifeGain) yield return HandleLifeGain();
        else if (_ability.effect == Effect.CardDraw) yield return HandleCardDraw();
        else if (_ability.effect == Effect.PriceReduction) yield return HandlePriceReduction();
        else if (_ability.effect == Effect.MoneyGain) yield return HandleMoneyGain();

        _abilitySource.RpcEffectHighlight(false);
    }

    private IEnumerator HandleCardDraw(){
        _abilitySource.Owner.DrawCards(_ability.amount);

        yield return new WaitForSeconds(SorsTimings.effectProjectile);

        yield return null;
    }

    private IEnumerator HandlePriceReduction()
    {
        // convert _ability.target (EffectTarget enum) to CardType enum
        var cardType = (CardType)Enum.Parse(typeof(CardType), _ability.target.ToString());
        var reduction = _ability.amount;

        _turnManager.PlayerGetsMarketBonus(_abilitySource.Owner, cardType, reduction);

        yield return new WaitForSeconds(SorsTimings.effectProjectile);

        yield return null;
    }

    private IEnumerator HandleMoneyGain()
    {
        _abilityTarget = _abilitySource.Owner.GetEntity();
        print($"MoneyGain, {_abilitySource.Title} -> {_abilityTarget.Title} : {_ability.amount}");
        _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.MoneyGain);
        yield return new WaitForSeconds(SorsTimings.effectProjectile);
        // yield return new WaitUntil(() => !_abilitiesVFXSystem.IsPlaying);

        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.MoneyGain);
        _abilitySource.Owner.Cash += _ability.amount;
    }

    private IEnumerator HandleLifeGain()
    {
        _abilityTarget = _abilitySource.Owner.GetEntity();
        print($"LifeGain, {_abilitySource.Title} -> {_abilityTarget.Title} : {_ability.amount}");
        _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.LifeGain);
        yield return new WaitForSeconds(SorsTimings.effectProjectile);
        // yield return new WaitUntil(() => !_abilitiesVFXSystem.IsPlaying);

        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.LifeGain);
        _abilitySource.Owner.Health += _ability.amount;
    }

    private IEnumerator HandleDamage()
    {
        // Without target
        if(_abilityTarget == null) {
            // TODO: Bug here : null object -> opponent entity as target ?
            // TODO: Compare with combat clash and targeting 
            if(_ability.target == EffectTarget.Opponent) _abilityTarget = _turnManager.GetOpponentPlayer(_abilitySource.Owner).GetEntity();
            else if (_ability.target == EffectTarget.Player) _abilityTarget = _abilitySource.Owner.GetEntity();
        }
        print($"HandleDamage, {_abilitySource.Title} -> {_abilityTarget.Title} : {_ability.amount}");
        
        // Is still null for (_ability.target == EffectTarget.Self) -> only want hit animation
        if (_abilityTarget != null){
            _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.Damage);
            // _abilitiesVFXSystem.IsPlaying = true;
            yield return new WaitForSeconds(SorsTimings.effectProjectile);

            // yield return new WaitUntil(() => !_abilitiesVFXSystem.IsPlaying);
        }
        
        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.Damage);
        _abilityTarget.EntityTakesDamage(_ability.amount, _abilitySource.CardInfo.keywordAbilities.Contains(Keywords.Deathtouch));
    }


    internal void SetSource(BattleZoneEntity source, Ability ability) {
        _abilitySource = source;
        _ability = ability;
    }
    internal void SetTarget(BattleZoneEntity target)
    {
        _abilityTarget = target;
        _abilitySource.RpcDeclaredTarget(_abilityTarget);
    }
    internal void Reset()
    {
        _abilitySource = null;
        _abilityTarget = null;
    }
}
