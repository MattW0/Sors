using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CreatureEntity : BattleZoneEntity //, IPointerClickHandler
{
    [SerializeField] private CreatureEntityUI _creatureUI;
    private List<Keywords> _keywordAbilities;
    public void SetKeywords(List<Keywords> keywords) => _keywordAbilities = keywords;
    public List<Keywords> GetKeywords() => _keywordAbilities;
    
    private bool _canAct;
    public bool CanAct 
    {
        get => _canAct;
        set {
            _canAct = value;
            if(value) _creatureUI.CanAct(true);
            else _creatureUI.CanAct(false);
        }
    }
    [SerializeField] private bool _isAttacking;
    public bool IsAttacking 
    { 
        get => _isAttacking; 
        set {
            _isAttacking = value;
            if (value) _creatureUI.ShowAsAttacker();
            else _creatureUI.CreatureIdle();
        }
    }

    [SerializeField] private bool _isBlocking;
    public bool IsBlocking 
    { 
        get => _isBlocking; 
        set {
            _isBlocking = value;
            if (value) _creatureUI.ShowAsBlocker();
            else _creatureUI.CreatureIdle();
        }
    }

    public void CheckIfCanAct(){
        if (!isOwned) return;

        if (IsAttacking) return;
        CanAct = true;
    }


    // TODO: Move to BZE because technologies taking damage
    [ClientRpc]
    public void RpcSetCombatHighlight() => _creatureUI.CombatHighlight();

    [ClientRpc]
    public void RpcResetAfterCombat(){
        CanAct = false;
        IsAttacking = false;

        _creatureUI.ResetHighlight();
    }
}
