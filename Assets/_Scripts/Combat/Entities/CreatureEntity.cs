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
    

    public bool CanAct { get; private set; }
    [SerializeField] private bool _isAttacking;
    public bool IsAttacking
    {
        get => _isAttacking;
        set
        {
            _isAttacking = value;
            if(_isAttacking) SpawnAttackerArrow();
            else RemoveAttackerArrow();
        } 
    }

    public void CheckIfCanAct(){
        if (!isOwned) return;

        if (IsAttacking) return;
        CanAct = true;
        _creatureUI.Highlight(true);
    }

    [ClientRpc]
    public void RpcOpponentCreatureIsAttacker(){
        if (isOwned) return;

        _isAttacking = true;
        _creatureUI.ShowAsAttacker(true);
        _creatureUI.TapOpponentCreature();
    }

    public void LocalPlayerIsReady(){
        // to show ui change when local player presses ready button
        CanAct = false;

        if (IsAttacking) _creatureUI.ShowAsAttacker(true);
        else _creatureUI.Highlight(false);
    }

    [ClientRpc]
    public void RpcSetCombatHighlight() => _creatureUI.CombatHighlight();
    public void SetHighlight(bool active) => _creatureUI.Highlight(active);

    [ClientRpc]
    public void RpcResetAfterCombat(){
        CanAct = false;
        IsAttacking = false;

        _creatureUI.ResetHighlight();
    }
}
