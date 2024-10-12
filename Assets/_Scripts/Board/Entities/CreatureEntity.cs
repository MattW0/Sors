using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class CreatureEntity : BattleZoneEntity
{
    [SerializeField] private CreatureEntityUI _creatureUI;
    private List<Traits> _traits;
    public List<Traits> GetTraits() => _traits;
    public static event Action<Transform, Transform> OnDeclaredAttack;
    public static event Action<Transform, Transform> OnDeclaredBlock;

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

    private void Start()
    {
        DropZoneManager.OnDeclareAttackers += DeclareAttackers;
        DropZoneManager.OnDeclareBlockers += DeclareBlockers;
        DropZoneManager.OnResetEntityUI += ResetCreatureUI;
    }

    public void InitializeCreature(int attack, List<Traits> traits)
    {
        _traits = traits;
        _attack = attack;
    }

    private void DeclareAttackers(bool begin)
    {
        if(begin) CheckIfCanAct();
        else CanAct = false;
    }

    private void DeclareBlockers(bool begin)
    {
        if(begin) {
            if(isOwned) CheckIfCanAct();
            else {
                if(IsAttacking) IsTargetable = true;
            }
        }
        else CanAct = false;
    }

    [ClientRpc]
    public void RpcDeclaredAttack(BattleZoneEntity target)
    {
        IsAttacking = true;
        OnDeclaredAttack?.Invoke(transform, target.transform);
    }

    [ClientRpc]
    public void RpcDeclaredBlock(BattleZoneEntity target)
    {
        IsBlocking = true;
        OnDeclaredBlock?.Invoke(transform, target.transform);
    }

    private void CheckIfCanAct()
    {
        if (!isOwned) return;

        if (IsAttacking) return;
        CanAct = true;
    }

    private void ResetCreatureUI(){
        CanAct = false;
        IsAttacking = false;
        IsBlocking = false;

        _creatureUI.ResetHighlight();
    }

    private void OnDestroy()
    {
        DropZoneManager.OnResetEntityUI -= ResetCreatureUI;
        DropZoneManager.OnDeclareAttackers -= DeclareAttackers;
        DropZoneManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
