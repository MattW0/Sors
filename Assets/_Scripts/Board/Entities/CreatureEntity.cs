using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class CreatureEntity : BattleZoneEntity
{
    [SerializeField] private EntityUI _ui;
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
            if(value) _ui.Highlight(HighlightType.Default);
            else _ui.Highlight(HighlightType.None);
        }
    }
    [SerializeField] private bool _isAttacking;
    public bool IsAttacking 
    { 
        get => _isAttacking; 
        set {
            _isAttacking = value;
            if (value) _ui.Attacking();
            else _ui.Highlight(HighlightType.None);
        }
    }
    [SerializeField] private bool _isBlocking;
    public bool IsBlocking 
    { 
        get => _isBlocking; 
        set {
            _isBlocking = value;
            if (value) _ui.Blocking();
            else _ui.Highlight(HighlightType.None);
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
        if(begin) CheckIfCanAct(true);
        else CanAct = false;
    }

    private void DeclareBlockers(bool begin)
    {
        if(!begin) {
            CanAct = false;
            return;
        }

        if(isOwned) CheckIfCanAct(false);
        else {
            if(IsAttacking) IsTargetable = true;
        }
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

    private void CheckIfCanAct(bool attackStep)
    {
        if (!isOwned) return;

        if (IsAttacking) return;

        // Defensive creatures can only block and offensive creatures can only attack
        CanAct = attackStep ? ! _traits.Contains(Traits.Devensive) : ! _traits.Contains(Traits.Offensive);
    }

    private void ResetCreatureUI()
    {
        CanAct = false;
        IsAttacking = false;
        IsBlocking = false;
    }

    private void OnDestroy()
    {
        DropZoneManager.OnResetEntityUI -= ResetCreatureUI;
        DropZoneManager.OnDeclareAttackers -= DeclareAttackers;
        DropZoneManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
