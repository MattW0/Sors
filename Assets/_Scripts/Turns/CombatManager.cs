using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatState state;
    public static event Action<CombatState> OnCombatStateChanged;

    public void UpdateCombatState(CombatState newState){
        state = newState;
        
        OnCombatStateChanged?.Invoke(state);

        switch(state){
            case CombatState.Idle:
                break;
            case CombatState.Attackers:
                DeclaringAttackers();
                break;
            case CombatState.Blockers:
                DeclaringBlockers();
                break;
            case CombatState.Damage:
                DealDamage();
                break;
            case CombatState.CleanUp:
                ResolveCombat();
                break;
            default:
                print("<color=red>Invalid turn state</color>");
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void DeclaringAttackers()
    {
        UpdateCombatState(CombatState.Blockers);
    }
    
    private void DeclaringBlockers()
    {
        UpdateCombatState(CombatState.Damage);
    }
    
    private void DealDamage()
    {
        UpdateCombatState(CombatState.CleanUp);
    }
    
    private void ResolveCombat()
    {
        print("Combat finished");
        
        UpdateCombatState(CombatState.Idle);
        TurnManager.Instance.CombatCleanUp();
    }
}

public enum CombatState{
    Idle,
    Attackers,
    Blockers,
    Damage,
    CleanUp
}
