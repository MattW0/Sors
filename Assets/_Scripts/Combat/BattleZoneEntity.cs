using System;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    private BoardManager _boardManager;
    public PlayerManager Owner { get; private set; }
    public PlayerManager Target { get; private set; }
    
    private bool _canAct;
    [field: SyncVar, SerializeField] public bool IsAttacking { get; private set; }
    [field: SyncVar] public bool IsBlocking { get; private set; }

    [SerializeField] private CombatState currentState;

    // private CombatState CurrentState
    // {
    //     get => currentState;
    //     set => currentState = value;
    // }

    private CardStats _cardStats;

    [SerializeField] private EntityUI entityUI;
    [SerializeField] private BlockerArrowHandler arrowHandler;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] public int attack;

    [SyncVar] private int _health;
    public int Health
    {
        get => _health;
        set
        {
            print(Title + " takes damage: " + value);
            _health -= value;
            entityUI.SetHealth(_health);
            if (_health <= 0) Die();
        }
    }

    [ClientRpc]
    public void RpcSpawnEntity(PlayerManager owner, CardInfo cardInfo, int holderNumber)
    {
        _boardManager = BoardManager.Instance;
        
        Owner = owner;
        Target = owner.opponent;
        
        SetStats(cardInfo);
        entityUI.MoveToHolder(holderNumber, hasAuthority);
    }

    private void SetStats(CardInfo cardInfo)
    {
        Title = cardInfo.title;
        attack = cardInfo.attack;
        _health = cardInfo.health;
        
        entityUI.SetEntityUI(cardInfo);
    }
    
    public void CanAct(CombatState combatState)
    {
        currentState = combatState;
        if (!hasAuthority || IsAttacking) return;
        
        _canAct = true;
        entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasAuthority || !_canAct) return;

        switch (currentState)
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
        entityUI.ShowAsAttacker(true);
    }
    
    [TargetRpc]
    public void TargetIsAttacker(NetworkConnection connection)
    {
        entityUI.ShowAsAttacker(true);
    }

    private void ClickBlocker()
    {
        IsBlocking = !IsBlocking;
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
    public void RpcTakesDamage(int value)
    {
        Health -= value;
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
    
    [Server]
    public bool ServerIsAttacker()
    {
        // Special case if server is attacking: 
        // IsAttacking is not correctly updated there,
        // thus we check if this entity is an attacker
        // ie. is in _boardManager.attackers
        //
        // if yes: return true -> we do not block anything
        return _boardManager.attackers.Contains(this);
        // grausig ...
    }
}
