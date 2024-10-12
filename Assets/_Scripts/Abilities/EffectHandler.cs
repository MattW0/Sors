using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EffectHandler : MonoBehaviour
{
    private AbilitiesVFXSystem _abilitiesVFXSystem;
    private TurnManager _turnManager;
    private PlayerInterfaceManager _playerInterfaceManager;
    private Ability _ability;
    private BattleZoneEntity _abilitySource;
    // TODO: Make this a list for multiple targets
    private BattleZoneEntity _abilityTarget;
    private bool _targetSelf = false;
    private WaitForSeconds _wait = new WaitForSeconds(SorsTimings.effectProjectile);

    private void Start()
    {
        _turnManager = TurnManager.Instance;
        _abilitiesVFXSystem = AbilitiesVFXSystem.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    internal IEnumerator Execute()
    {
        // Sanity check
        if (_abilitySource == null) yield break;

        print($"Executing ability: " + _ability.ToString());

        if(_abilityTarget == null) EvaluateTarget();
        _playerInterfaceManager.RpcLog(_ability.ToString(), LogType.EffectTrigger);

        if (_ability.effect == Effect.Damage) yield return HandleDamage();
        else if (_ability.effect == Effect.LifeGain) yield return HandleLifeGain();
        else if (_ability.effect == Effect.CardDraw) yield return HandleCardDraw();
        else if (_ability.effect == Effect.PriceReduction) yield return HandlePriceReduction();
        else if (_ability.effect == Effect.MoneyGain) yield return HandleMoneyGain();
    }

    private IEnumerator HandleCardDraw(){
        _abilitySource.Owner.DrawCards(_ability.amount);

        yield return _wait;
    }

    private IEnumerator HandlePriceReduction()
    {
        // convert _ability.target (EffectTarget enum) to CardType enum
        var cardType = (CardType)Enum.Parse(typeof(CardType), _ability.target.ToString());
        var reduction = _ability.amount;

        _turnManager.PlayerGetsMarketBonus(_abilitySource.Owner, cardType, reduction);

        yield return _wait;
    }

    private IEnumerator HandleMoneyGain()
    {
        _abilityTarget = _abilitySource.Owner.GetEntity();
        _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.MoneyGain);
        yield return _wait;

        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.MoneyGain);
        _abilitySource.Owner.Cash += _ability.amount;
    }

    private IEnumerator HandleLifeGain()
    {
        _abilityTarget = _abilitySource.Owner.GetEntity();
        _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.LifeGain);
        yield return _wait;

        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.LifeGain);

        // TODO: Lifegain for creatures and entities
        _abilitySource.Owner.Health += _ability.amount;
    }

    private IEnumerator HandleDamage()
    {        
        // Only play projectile if not targeting self
        if (! _targetSelf){
            _abilitiesVFXSystem.RpcPlayProjectile(_abilitySource, _abilityTarget, Effect.Damage);
            yield return _wait;
        }
        
        _abilitiesVFXSystem.RpcPlayHit(_abilityTarget, Effect.Damage);
        _abilityTarget.EntityTakesDamage(_ability.amount, _abilitySource.CardInfo.traits.Contains(Traits.Deathtouch));
    }

    internal void SetSource(BattleZoneEntity source, Ability ability)
    {
        _abilitySource = source;
        _ability = ability;
    }
    internal void SetTarget(BattleZoneEntity target)
    {
        _abilityTarget = target;
        _abilitySource.RpcDeclaredTarget(_abilityTarget);
    }

    private void EvaluateTarget()
    {
        if(_ability.target == EffectTarget.Opponent) _abilityTarget = _turnManager.GetOpponentPlayer(_abilitySource.Owner).GetEntity();
        else if (_ability.target == EffectTarget.You) _abilityTarget = _abilitySource.Owner.GetEntity();
        else if (_ability.target == EffectTarget.Self) {
            _abilityTarget = _abilitySource;
            _targetSelf = true;
        }

        else if (_ability.effect == Effect.PriceReduction) _abilityTarget = _abilitySource.Owner.GetEntity();
    }

    internal void Reset()
    {
        _abilitySource = null;
        _abilityTarget = null;
        _targetSelf = false;
    }
}
