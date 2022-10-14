using System;
using System.Collections;
using System.Collections.Generic;
using CardDecoder;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    [SerializeField] private bool isPlayer;
    [SerializeField, SyncVar] private bool isDeployed;

    public static event Action<BattleZoneEntity, bool> Attack;
    private List<BattleZoneEntity> _attackers;

    public bool IsDeployed
    {
        get => isDeployed;
        set
        {
            isDeployed = value;
            if (isDeployed) Deploy();
            else GoToDiscard();
        }
    }

    private bool _canAct;
    private bool _isAttacking;
    private CombatState _currentState;

    private PlayerManager _player;
    private CardStats _cardStats;
    private CardUI _cardUI;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] private int _attack;
    [SyncVar] private int _health;

    private void Awake()
    {
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    private void Deploy()
    {
        if (isPlayer) {
            _player = gameObject.GetComponent<PlayerManager>();
            Title = _player.playerName;
            _attack = 0;
            _health = _player.Health;
        } else {
            _cardStats = gameObject.GetComponent<CardStats>();
            Title = _cardStats.cardInfo.title;
            _attack = _cardStats.cardInfo.attack;
            _health = _cardStats.cardInfo.health;
        }
    }
    
    public void CanAct(CombatState combatState)
    {
        _currentState = combatState;
        
        if (!hasAuthority) return;
        if (!isDeployed) return;
        if (_isAttacking) return;

        _canAct = true;
        _cardUI.Highlight(true, Color.white);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasAuthority) return;
        if (!_canAct) return;

        switch (_currentState)
        {
            case CombatState.Attackers:
                ClickAttacker();
                break;
            case CombatState.Blockers:
                ClickBlocker();
                break;
        }
    }

    private void ClickAttacker()
    {
        if (!_isAttacking) {
            _isAttacking = true;
            Attack?.Invoke(this, true);
            _cardUI.TapCreature();
        } else {
            _isAttacking = false;
            Attack?.Invoke(this, false);
            _cardUI.UntapCreature();
        }
    }

    private void ClickBlocker()
    {
        
    }

    private void ChangeAttack(int amount)
    {
        _attack += amount;
        _attack = Math.Max(0, _attack);
    }
    
    private void ChangeHealth(int amount)
    {
        _health += amount;
        if (_health <= 0) Die();
    }

    private void Die()
    {
        print("Entity " + Title + " dies.");
        IsDeployed = false;
    }

    private void GoToDiscard()
    {
        print("Entity " + Title + " is put into Discard.");
        // gameObject.GetComponent<CardMover>().MoveToDestination()
    }
}
