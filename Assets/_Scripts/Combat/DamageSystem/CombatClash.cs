using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CombatClash
{
    private CombatVFXSystem _combatVFXSystem = CombatVFXSystem.Instance;
    private Transform _source;
    private Transform _target;
    private int _damageFromSource;
    private int _damageFromTarget;
    private bool _isClash;

    public bool IsDone { get; private set; }

    // Need to pass attack damage for trample and _target groups
    public CombatClash(CreatureEntity a, CreatureEntity b, int aDmg, int bDmg)
    {
        _source = a.gameObject.transform;
        _target = b.gameObject.transform;
        _damageFromSource = aDmg;
        _damageFromTarget = bDmg;

        _isClash = true;
    }

    public CombatClash(CreatureEntity a, BattleZoneEntity t, int aDmg, int bDmg = 0)
    {
        _source = a.gameObject.transform;
        _target = t.gameObject.transform;
        _damageFromSource = aDmg;
    }

    public IEnumerator Execute() 
    {

        // yield return _combatVFXSystem.PlayAttackCR(_source, _target);
        Debug.Log($"Attack Animation : {_source.position} -> {_target.position}");
        _combatVFXSystem.PlayAttack(_source, _target);
        yield return new WaitForSeconds(2f);

        Debug.Log($"Damage Animation at {_target.position} with {_damageFromSource} damage");
        yield return _combatVFXSystem.PlayDamage(_target.position, _damageFromSource);
        yield return new WaitForSeconds(0.5f);

        if (_isClash)
        {
            Debug.Log($"Attack Animation : {_source.position} -> {_target.position}");
            _combatVFXSystem.PlayAttack(_source, _target);
            yield return new WaitForSeconds(2f);

            Debug.Log($"Damage Animation at {_source} with {_damageFromTarget} damage");
            yield return _combatVFXSystem.PlayDamage(_source.position, _damageFromTarget);
            yield return new WaitForSeconds(0.5f);
        }
        
        IsDone = true;
    }

    public override string ToString()
    {
        return $"CombatClash: {_source.name} vs {_target.name} with {_damageFromSource} damage\nIsDone: {IsDone}";
    }
}
