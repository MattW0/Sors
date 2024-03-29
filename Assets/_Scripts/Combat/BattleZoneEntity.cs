using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    public PlayerManager Owner { get; private set; }
    public PlayerManager Target { get; private set; }
    private BoardManager _boardManager;


    [field: Header("Combat Stats")]
    public bool CanAct { get; private set; }
    [SerializeField] public bool IsAttacking;
    public List<Keywords> _keywordAbilities { get; private set; }

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
    
    private int _health;
    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            RpcSetHealth(_health);
            if (_health <= 0) Die();
        }
    }

    [SyncVar] private int _points;
    private int Points
    {
        get => _points;
        set
        {
            _points = value;
            entityUI.SetPoints(_points);
        }
    }
    
    public event Action<PlayerManager, BattleZoneEntity> OnDeath;

    [ClientRpc]
    public void RpcSpawnEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo, int holderNumber){        
        Owner = owner;
        Target = opponent;
        
        SetStats(cardInfo);
        entityUI.MoveToHolder(holderNumber, isOwned);

        if (isServer) _boardManager = BoardManager.Instance;
    }

    private void SetStats(CardInfo cardInfo)
    {
        Title = cardInfo.title;
        _attack = cardInfo.attack;
        _health = cardInfo.health;
        _points = cardInfo.points;

        _keywordAbilities = cardInfo.keyword_abilities;
        
        entityUI.SetEntityUI(cardInfo);
    }
    
    public void CheckIfCanAct(CombatState newState)
    {
        if (!isOwned) return;
        combatState = newState;

        if (IsAttacking) return;
        CanAct = true;
        entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isOwned || !CanAct) return;
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
            entityUI.UntapCreature(highlight: true);
        }
    }
    
    [ClientRpc]
    public void RpcIsAttacker()
    {
        IsAttacking = true;
        CanAct = false;
        entityUI.ShowAsAttacker(true);
        
        if (isOwned) return;
        entityUI.TapOpponentCreature();
    }

    public void LocalPlayerIsReady(){
        // to show ui change when local player presses ready button
        CanAct = false;

        if (IsAttacking) entityUI.ShowAsAttacker(true);
        else entityUI.Highlight(false);
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
        if (!isOwned) return;
        arrowHandler.HandleFoundEnemyTarget(attacker);
    }
    
    [ClientRpc]
    public void RpcShowOpponentsBlockers(List<BattleZoneEntity> blockers)
    {
        if (!isOwned) return;
        foreach (var blocker in blockers)
        {
            arrowHandler.ShowOpponentBlocker(blocker.gameObject);
        }
    }

   
    public void TakesDamage(int value, bool deathtouch)
    {
        if (deathtouch){ 
            Health = 0;
            return;
        }
        Health -= value;
    }

    [ClientRpc]
    private void RpcSetHealth(int value)=> entityUI.SetHealth(value);

    private void Die() => OnDeath?.Invoke(Owner, this);

    [ClientRpc]
    public void RpcRetreatAttacker(){
        if (isOwned) entityUI.UntapCreature(highlight: false);
        else {
            entityUI.UntapOpponentCreature();
            entityUI.ShowAsAttacker(false);
        }
    }

    public void ResetAfterCombat()
    {
        CanAct = false;
        IsAttacking = false;
        
        entityUI.ShowAsAttacker(false);
        entityUI.Highlight(false);
    }
}
