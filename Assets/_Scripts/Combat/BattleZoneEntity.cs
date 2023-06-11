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
    [SerializeField] private EntityUI _entityUI;

    [field: Header("Combat Stats")]
    public bool CanAct { get; private set; }
    [SerializeField] public bool IsAttacking;
    public List<Keywords> _keywordAbilities { get; private set; }
    
    [Header("Helper Fields")]
    public BlockerArrowHandler arrowHandler;
    [SerializeField] private CombatState _combatState;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] private int _attack;
    public int Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            _entityUI.SetAttack(_attack);
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
            _entityUI.SetPoints(_points);
        }
    }
    
    public event Action<PlayerManager, BattleZoneEntity> OnDeath;

    [ClientRpc]
    public void RpcInitializeEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo){        
        Owner = owner;
        Target = opponent;
        
        SetStats(cardInfo);
        if (isServer) _boardManager = BoardManager.Instance;
    }

    private void SetStats(CardInfo cardInfo)
    {
        Title = cardInfo.title;
        _attack = cardInfo.attack;
        _health = cardInfo.health;
        _points = cardInfo.points;

        _keywordAbilities = cardInfo.keywordAbilities;
        
        _entityUI.SetEntityUI(cardInfo);
    }
    
    public void CheckIfCanAct(CombatState newState)
    {
        if (!isOwned) return;
        _combatState = newState;

        if (IsAttacking) return;
        CanAct = true;
        _entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData){
        if (!isOwned || !CanAct) return;
        if (_combatState != CombatState.Attackers) return;

        // only in Attackers since blockerArrowHandler handles Blockers phase
        ClickAttacker();
    }

    private void ClickAttacker(){
        if (!IsAttacking) {
            IsAttacking = true;
            _entityUI.TapCreature();
        } else {
            IsAttacking = false;
            _entityUI.UntapCreature(highlight: true);
        }
    }
    
    [ClientRpc]
    public void RpcIsAttacker(){
        IsAttacking = true;
        CanAct = false;
        _entityUI.ShowAsAttacker(true);
        
        if (isOwned) return;
        _entityUI.TapOpponentCreature();
    }

    public void LocalPlayerIsReady(){
        // to show ui change when local player presses ready button
        CanAct = false;

        if (IsAttacking) _entityUI.ShowAsAttacker(true);
        else _entityUI.Highlight(false);
    } 
    
    [TargetRpc]
    public void TargetIsAttacker(NetworkConnection connection){
        CanAct = false;
        _entityUI.ShowAsAttacker(true);
    }
    
    [TargetRpc]
    public void TargetCanNotAct(NetworkConnection connection){
        CanAct = false;
        _entityUI.Highlight(false);
    }

    [ClientRpc]
    public void RpcBlockerDeclared(BattleZoneEntity attacker){
        if (!isOwned) return;
        arrowHandler.HandleFoundEnemyTarget(attacker);
    }
    
    [ClientRpc]
    public void RpcShowOpponentsBlockers(List<BattleZoneEntity> blockers){
        if (!isOwned) return;
        foreach (var blocker in blockers)
        {
            arrowHandler.ShowOpponentBlocker(blocker.gameObject);
        }
    }

    [Server]
    public void TakesDamage(int value, bool deathtouch){
        if (deathtouch){ 
            Health = 0;
            return;
        }
        Health -= value;
    }

    [ClientRpc]
    private void RpcSetHealth(int value)=> _entityUI.SetHealth(value);

    private void Die() => OnDeath?.Invoke(Owner, this);

    [ClientRpc]
    public void RpcRetreatAttacker(){
        if (isOwned) _entityUI.UntapCreature(highlight: false);
        else {
            _entityUI.UntapOpponentCreature();
            _entityUI.ShowAsAttacker(false);
        }
    }

    [ClientRpc]
    public void RpcSetCombatHighlight() => _entityUI.CombatHighlight();
    public void SetHighlight(bool active) => _entityUI.Highlight(active);

    public void SetPosition(bool isMine){
        if(isMine) return;
        
        _entityUI.UntapOpponentCreature();
    }

    public void ResetAfterCombat(){
        CanAct = false;
        IsAttacking = false;
        
        _entityUI.ShowAsAttacker(false);
        _entityUI.ResetHighlight();
    }
}
