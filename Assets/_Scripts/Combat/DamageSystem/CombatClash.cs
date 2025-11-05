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
    public static event Action<int> OnFinishClash;

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
        // Debug.Log("Execute " + this);
        await ExecuteDamage(_source, _target, _damageFromSource);

        if (! IsClash) return;
        
        // Target must be creature during a clash
        await ExecuteDamage(_target as CreatureEntity, _source, _damageFromTarget);
    }

    private async UniTask ExecuteDamage(CreatureEntity source, BattleZoneEntity target, int damage)
    {
        // Debug.Log($"ExecuteDamage: source={source}, target={target}, damage={damage}");
        OnPlayAttack?.Invoke(source.transform, target.transform);
        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.attackTime));

        OnPlayDamage?.Invoke(target.transform);
        await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.damageTime));

        target.EntityTakesDamage(damage, source.GetTraits().Contains(Trait.Deathtouch));
        OnFinishClash?.Invoke(source.ID);
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
