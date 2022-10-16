using System;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    public static event Action<BattleZoneEntity, bool> Attack;

    private bool _canAct;
    private bool _isAttacking;
    private CombatState _currentState;

    private PlayerManager _player;
    private CardStats _cardStats;

    [SerializeField] private EntityUI entityUI;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] private int _attack;
    [SyncVar] private int _health;

    [ClientRpc]
    public void RpcSpawnEntity(CardInfo cardInfo, int holderNumber)
    {
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
    
    public void CanAct(CombatState combatState)
    {
        _currentState = combatState;
        print("Can " + Title + " act?");
        
        if (!hasAuthority || _isAttacking) return;
        
        _canAct = true;
        entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasAuthority || !_canAct) return;

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
            entityUI.TapCreature();
        } else {
            _isAttacking = false;
            Attack?.Invoke(this, false);
            entityUI.UntapCreature();
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
    }

    private void GoToDiscard()
    {
        print("Entity " + Title + " is put into Discard.");
        // gameObject.GetComponent<CardMover>().MoveToDestination()
    }
}
