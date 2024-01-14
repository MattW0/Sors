using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(BlockerArrowHandler))]
public class CreatureEntity : BattleZoneEntity
{
    [SerializeField] private CreatureEntityUI _creatureUI;
    private List<Keywords> _keywordAbilities;
    public List<Keywords> GetKeywords() => _keywordAbilities;
    
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
            if(value) _creatureUI.Highlight(true);
            else _creatureUI.Highlight(false);
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

    private void Start(){
        DropZoneManager.OnResetEntityUI += ResetCreatureUI;
    }

    public void InitializeCreature(int attack, List<Keywords> keywords){
        _keywordAbilities = keywords;
        _attack = attack;
        blockerArrowHandler = GetComponent<BlockerArrowHandler>();
    }

    [ClientRpc]
    public override void RpcCombatStateChanged(CombatState newState){
        base.RpcCombatStateChanged(newState);
        blockerArrowHandler.CombatStateChanged(newState);
    }

    public void CheckIfCanAct(){
        if (!isOwned) return;

        if (IsAttacking) return;
        CanAct = true;
    }

    [TargetRpc]
    public void TargetDeclaredAttack(NetworkConnection conn, BattleZoneEntity target)
    {
        CanAct = false;
        IsAttacking = true;
        attackerArrowHandler.HandleFoundTarget(target.transform);
    }

    [ClientRpc]
    public void RpcDeclaredAttack(BattleZoneEntity target)
    {
        CanAct = false;
        IsAttacking = true;
        attackerArrowHandler.HandleFoundTarget(target.transform);
    }

    [TargetRpc]
    public void TargetDeclaredBlock(NetworkConnection conn, BattleZoneEntity target)
    {
        CanAct = false;
        IsBlocking = true;
        blockerArrowHandler.HandleFoundTarget(target.transform);
    }

    [ClientRpc]
    public void RpcDeclaredBlock(BattleZoneEntity target)
    {
        CanAct = false;
        blockerArrowHandler.HandleFoundTarget(target.transform);
    }

    // [ClientRpc]
    public void ResetCreatureUI(){
        CanAct = false;
        IsAttacking = false;
        IsBlocking = false;

        _creatureUI.ResetHighlight();
    }

    // [ClientRpc]
    // private void RpcSetAttack(int value) => _entityUI.SetAttack(value);

    private void OnDestroy()
    {
        DropZoneManager.OnResetEntityUI -= ResetCreatureUI;
    }
}
