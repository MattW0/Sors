using System;
using Mirror;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CombatClash
{
    public CreatureEntity _source;
    public BattleZoneEntity _target;
    public bool IsClash => _target is CreatureEntity;
    public int AggressorID => _source.Owner.ID;
    private int _damageFromSource;
    private int _damageFromTarget;

    public static event Action<Transform> OnPlayDamage;
    public static event Action<Transform, Transform> OnPlayAttack;

    // Need to pass attack damage for trample and _target groups
    public CombatClash(CreatureEntity attacker, CreatureEntity blocker, int aDmg, int bDmg)
    {
        _source = attacker;
        _target = blocker;
        _damageFromSource = aDmg;
        _damageFromTarget = bDmg;
    }

    public CombatClash(CreatureEntity a, BattleZoneEntity t, int aDmg)
    {
        _source = a;
        _target = t;
        _damageFromSource = aDmg;
    }

    public async UniTask ExecuteCombatClash()
    {
        OnPlayAttack?.Invoke(_source.gameObject.transform, _target.gameObject.transform);
        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.attackTime));

        OnPlayDamage?.Invoke(_target.gameObject.transform);
        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.damageTime));

        _target.EntityTakesDamage(_damageFromSource, _source.GetTraits().Contains(Traits.Deathtouch));

        if (IsClash)
        {
            OnPlayAttack?.Invoke(_target.gameObject.transform, _source.gameObject.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.attackTime));

            OnPlayDamage?.Invoke(_source.gameObject.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.damageTime));
            
            // Can't be player if it is clash
            var targetCe = (CreatureEntity) _target;
            _source.EntityTakesDamage(_damageFromTarget, targetCe.GetTraits().Contains(Traits.Deathtouch));
        }
    }

    public override string ToString()
    {
        var log = "";
        if(IsClash) {
            log += $"{_source.Title} deals {_damageFromSource} to {_target.Title}.\n";
            log += $"{_target.Title} deals {_damageFromTarget} to {_source.Title}.";
        } else {
            log = $"{_source.Title} deals {_damageFromSource} to {_target.Title}.";
        }

        return log;
    }
}
