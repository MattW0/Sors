using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;


public class DamageSystem : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private PlayerInterfaceManager _playerInterfaceManager;
    private List<CombatClash> _clashes = new();
    private Dictionary<CreatureEntity, BattleZoneEntity> _attackerTarget = new();

    public void EvaluateBlocks(Dictionary<CreatureEntity, BattleZoneEntity> aT, Dictionary<CreatureEntity, CreatureEntity> bA)
    {
        _attackerTarget = aT;
        if (_attackerTarget.Count == 0){
            print("No attackers, skip to Clean Up");
            _combatManager.UpdateCombatState(TurnState.CombatCleanUp);
            return;
        }

        // print("AttackerTarget: " + _attackerTarget.Count);
        // print("BlockerAttacker: " + bA.Count);
        
        if (bA.Count == 0){
            EvaluateUnblocked();
            return;
        }

        int excessDamage = int.MinValue;
        var prevA = bA.First().Value;
        foreach(var (b, a) in bA)
        {
            // In the same blocker group -> Do normal damage
            if (!prevA.Equals(a)) {
                CheckTrampleDamage(prevA, excessDamage);

                excessDamage = int.MinValue;
                _attackerTarget.Remove(prevA);
            }

            if (excessDamage == int.MinValue)
                excessDamage = EvaluateClashDamage(a, b, a.Attack);
            else 
                excessDamage = EvaluateClashDamage(a, b, excessDamage);
            
            // print("Excess damage: " + excessDamage);
            prevA = a;
        }

        CheckTrampleDamage(prevA, excessDamage);
        _attackerTarget.Remove(prevA);

        EvaluateUnblocked();
    }

    private void EvaluateUnblocked()
    {
        foreach(var (a, t) in _attackerTarget){
            var clash = new CombatClash(a, t, a.Attack);
            _clashes.Add(clash);
        }

        ExecuteClashes().Forget();
    }

    // Calculate the outcome of a combat clash between two creature entities, considering their traits and attack damage. Returns the remaining attack damage after the clash.
    private int EvaluateClashDamage(CreatureEntity attacker, CreatureEntity blocker, int attackDamage)
    {
        var clash = new CombatClash(attacker, blocker, attackDamage, blocker.Attack);
        print("Add clash: " + clash);
        _clashes.Add(clash);

        return attackDamage - blocker.Health;
    }

    private void CheckTrampleDamage(CreatureEntity attacker, int excessDamage)
    {
        if (excessDamage <= 0 || ! attacker.GetTraits().Contains(Traits.Trample)) return;
        
        var target = _attackerTarget[attacker];
        // print($"Trample of '{attacker.Title}' with {excessDamage} excess damage on '{target.Title}'");
        _clashes.Add(new CombatClash(attacker, target, excessDamage));
        
    }

    private async UniTaskVoid ExecuteClashes() 
    {
        // Jänu was here : #@thisIsAComment ##xoxo

        foreach (var clash in _clashes)
        {
            _playerInterfaceManager.RpcLog(clash.AggressorID, clash.ToString());
            await clash.ExecuteCombatClash();
        }
        
        _clashes.Clear();
        _attackerTarget.Clear();
        _combatManager.UpdateCombatState(TurnState.CombatCleanUp);
    }

        // private bool EvaluateFirstStrike(CreatureEntity attacker, CreatureEntity blocker, int attackDamage, int blockDamage)
    // {
    //     var attackerTraits = attacker.GetTraits();
    //     var blockerTraits = blocker.GetTraits();

    //     // XOR: Neither or both have first strike
    //     if( ! attackerTraits.Contains(Traits.FirstStrike)
    //         ^ blockerTraits.Contains(Traits.FirstStrike))
    //             return true;

    //     // Only attacker has first strike, need to track trample
    //     if (attackerTraits.Contains(Traits.FirstStrike) 
    //         && blocker.Health - attackDamage > 0) 
    //             return true;

    //     // Only blocker has first strike
    //     if (blockerTraits.Contains(Traits.FirstStrike)
    //         && attacker.Health - blockDamage > 0)
    //             return true;

    //     return false;
    // }
}
 