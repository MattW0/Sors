using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    public PlayerManager Owner { get; private set; }
    public PlayerManager Target { get; private set; }

    [field: Header("Combat Stats")]
    public bool CanAct { get; private set; }
    [field: SyncVar, SerializeField] public bool IsAttacking { get; private set; }
    // [field: SyncVar] public bool IsBlocking { get; private set; }
    [SerializeField] private CombatState combatState;
    
    [Header("Other scripts")]
    [SerializeField] private EntityUI entityUI;
    public BlockerArrowHandler arrowHandler;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] private int _attack;
    public int Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            entityUI.SetAttack(_attack);
        }
    }
    
    [SyncVar] private int _health;
    private int Health
    {
        get => _health;
        set
        {
            _health = value;
            entityUI.SetHealth(_health);
            if (_health <= 0) Die();
        }
    }
    
    private BoardManager _boardManager;
    public event Action<BattleZoneEntity> OnDeath;

    [ClientRpc]
    public void RpcSpawnEntity(PlayerManager owner, PlayerManager opponent,
        CardInfo cardInfo, int holderNumber)
    {
        _boardManager = BoardManager.Instance;
        
        Owner = owner;
        Target = opponent;
        
        SetStats(cardInfo);
        entityUI.MoveToHolder(holderNumber, hasAuthority);
    }

    private void SetStats(CardInfo cardInfo)
    {
        Title = cardInfo.title;
        _attack = cardInfo.attack;
        _health = cardInfo.health;
        
        entityUI.SetEntityUI(cardInfo);
    }
    
    public void CheckIfCanAct(CombatState newState)
    {
        combatState = newState;
        if (!hasAuthority || IsAttacking) return;
        
        CanAct = true;
        entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasAuthority || !CanAct) return;
        if (combatState != CombatState.Attackers) return;

        // only in Attackers since blockerArrowHandler handles Blockers phase
        ClickAttacker();
    }

    private void ClickAttacker()
    {
        if (!IsAttacking) {
            IsAttacking = true;
            entityUI.TapCreature();
        } else {
            IsAttacking = false;
            entityUI.UntapCreature();
        }

        if (isServer) _boardManager.AttackerDeclared(this, IsAttacking);
        else CmdAttackerDeclared(IsAttacking);
    }

    [Command]
    private void CmdAttackerDeclared(bool isAttacking)
    {
        _boardManager.AttackerDeclared(this, isAttacking);
    }
    
    [ClientRpc]
    public void RpcIsAttacker()
    {
        CanAct = false;
        entityUI.ShowAsAttacker(true);
    }
    
    [TargetRpc]
    public void TargetIsAttacker(NetworkConnection connection)
    {
        CanAct = false;
        entityUI.ShowAsAttacker(true);
    }
    
    [TargetRpc]
    public void TargetCanNotAct(NetworkConnection connection)
    {
        CanAct = false;
        entityUI.Highlight(false);
    }

    [ClientRpc]
    public void RpcBlockerDeclared(BattleZoneEntity attacker)
    {
        if (hasAuthority)
        {
            arrowHandler.HandleFoundEnemyTarget(attacker);
        }
    }
    
    [ClientRpc]
    public void RpcShowOpponentsBlockers(List<BattleZoneEntity> blockers)
    {
        if (!hasAuthority) return;
        foreach (var blocker in blockers)
        {
            arrowHandler.ShowOpponentBlocker(blocker.gameObject);
        }
    }

    [ClientRpc]
    public void RpcTakesDamage(int value)
    {
        Health -= value;
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
    }

    [ClientRpc]
    public void RpcResetAfterCombat()
    {
        CanAct = false;
        IsAttacking = false;
        
        entityUI.ShowAsAttacker(false);
        entityUI.Highlight(false);
    }
    
    [Server] // grausig ...
    public bool ServerIsAttacker()
    {
        // Special case if server is attacking: 
        // IsAttacking is not correctly updated there,
        // thus we check if this entity is an attacker
        // ie. is in _boardManager.attackers
        //
        // if yes: return true -> we do not block anything
        return _boardManager.attackers.Contains(this);
    }
}
