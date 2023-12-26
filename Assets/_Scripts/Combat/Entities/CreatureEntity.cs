using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CreatureEntity : BattleZoneEntity
{
    [SerializeField] private CreatureEntityUI _creatureUI;
    private List<Keywords> _keywordAbilities;
    public List<Keywords> GetKeywords() => _keywordAbilities;
    private BlockerArrowHandler _blockerArrowHandler;


    public void InitializeCreature(int attack, List<Keywords> keywords){
        _keywordAbilities = keywords;
        _blockerArrowHandler = GetComponent<BlockerArrowHandler>();
        _attack = attack;
    }

    [ClientRpc]
    public override void RpcCombatStateChanged(CombatState newState){
        base.RpcCombatStateChanged(newState);
        _blockerArrowHandler.CombatStateChanged(newState);
    }
    
    [SerializeField] private int _attack;
    public int Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            RpcSetAttack(_attack);
        }
    }

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

    [ClientRpc]
    public void RpcDeclaredAttack(BattleZoneEntity target){
        CanAct = false;
        IsAttacking = true;
        // _attackerArrowHandler.HandleFoundTarget(target.transform);
        // PlayerInterfaceManager.Log($"  - {Title} attacks {target.Title}", LogType.Standard);
    }


    public void TargetDeclaredAttack(NetworkConnection conn, BattleZoneEntity target)
    {
        CanAct = false;
        // attackerArrowHandler.HandleFoundTarget(target.transform);
    }

    public void TargetDeclaredBlock(NetworkConnection conn, BattleZoneEntity target)
    {
        CanAct = false;
        IsBlocking = true;
        // _blockerArrowHandler.HandleFoundTarget(target.transform);
    }

    public void RpcDeclaredBlock(BattleZoneEntity target)
    {
        CanAct = false;
        // _blockerArrowHandler.HandleFoundTarget(target.transform);
    }

    [ClientRpc]
    private void RpcSetAttack(int value) => _entityUI.SetAttack(value);
}
