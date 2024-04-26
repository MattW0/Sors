using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CombatClash
{
    public CreatureEntity _source;
    public BattleZoneEntity _target;
    private int _damageFromSource;
    private int _damageFromTarget;
    public bool IsClash { get; private set; }
    public bool IsDone { get; private set; }
    private CombatVFXSystem _combatVFXSystem;

    // Need to pass attack damage for trample and _target groups
    public CombatClash(CreatureEntity attacker, CreatureEntity blocker, int aDmg, int bDmg)
    {
        _source = attacker;
        _target = blocker;
        _damageFromSource = aDmg;
        _damageFromTarget = bDmg;

        IsClash = EvaluateFirstStrike(attacker, blocker, aDmg, bDmg);
        _combatVFXSystem = CombatVFXSystem.Instance;
    }

    public CombatClash(CreatureEntity a, BattleZoneEntity t, int aDmg)
    {
        _source = a;
        _target = t;
        _damageFromSource = aDmg;
        _combatVFXSystem = CombatVFXSystem.Instance;
    }

    public IEnumerator Execute()
    {
        // Debug.Log($"Attack Animation : {_source.gameObject.transform.position} -> {_target.gameObject.transform.position}");
        _combatVFXSystem.RpcPlayAttack(_source, _target);
        yield return new WaitForSeconds(SorsTimings.attackTime);

        // Debug.Log($"Damage Animation at {_target.gameObject.transform.position} with {_damageFromSource} damage");
        _combatVFXSystem.RpcPlayDamage(_target, _damageFromSource);
        yield return new WaitForSeconds(SorsTimings.damageTime);
        _target.EntityTakesDamage(_damageFromSource, _source.GetTraits().Contains(Traits.Deathtouch));

        if (IsClash)
        {
            // Debug.Log($"Attack Animation : {_source.position} -> {_target.position}");
            _combatVFXSystem.RpcPlayAttack(_target, _source);
            yield return new WaitForSeconds(SorsTimings.attackTime);

            // Debug.Log($"Damage Animation at {_source} with {_damageFromTarget} damage");
            _combatVFXSystem.RpcPlayDamage(_source, _damageFromSource);
            yield return new WaitForSeconds(SorsTimings.damageTime);
            _source.Health -= _damageFromTarget;
        }
        
        IsDone = true;
    }

    private bool EvaluateFirstStrike(CreatureEntity attacker, CreatureEntity blocker, int attackDamage, int blockDamage)
    {
        var attackerTraits = attacker.GetTraits();
        var blockerTraits = blocker.GetTraits();

        // XOR: Neither or both have first strike
        if( ! attackerTraits.Contains(Traits.FirstStrike)
            ^ blockerTraits.Contains(Traits.FirstStrike))
                return true;

        // Only attacker has first strike, need to track trample
        if (attackerTraits.Contains(Traits.FirstStrike) 
            && blocker.Health - attackDamage > 0) 
                return true;

        // Only blocker has first strike
        if (blockerTraits.Contains(Traits.FirstStrike)
            && attacker.Health - blockDamage > 0)
                return true;

        return false;
    }

    public override string ToString()
    {
        var log = "";
        if(IsClash) {
            log += $"{_source.Title} -> {_target.Title} : {_damageFromSource}.\n";
            log += $"{_target.Title} -> {_source.Title} : {_damageFromTarget}";
        } else {
            log = $"{_source.Title} -> {_target.Title} : {_damageFromSource}.";
        }

        return log;
    }
}
