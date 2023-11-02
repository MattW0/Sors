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
        private set {
            _canAct = value;
            _creatureUI.CanAct(true);
        }
    }
    [SerializeField] private bool _isAttacking;
    public bool IsAttacking 
    { 
        get => _isAttacking; 
        set {
            _isAttacking = value;
            AttackerDeclared();
        }
    }
    public bool IsBlocking { get; set; }

    public void CheckIfCanAct(){
        if (!isOwned) return;

        if (IsAttacking) return;
        CanAct = true;
    }

    public void AttackerDeclared(){
        // to show ui change when local player presses ready button
        CanAct = false;
        if (IsAttacking) _creatureUI.ShowAsAttacker(true);
        else _creatureUI.ResetHighlight();
    }

    [ClientRpc]
    public void RpcSetCombatHighlight() => _creatureUI.CombatHighlight();

    [ClientRpc]
    public void RpcResetAfterCombat(){
        CanAct = false;
        IsAttacking = false;

        _creatureUI.ResetHighlight();
    }
}
