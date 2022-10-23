using System;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleZoneEntity : NetworkBehaviour, IPointerDownHandler
{
    private BoardManager _boardManager;
    public PlayerManager Owner { get; private set; }

    public BattleZoneEntity target;
    
    private bool _canAct;
    private bool _isAttacking;
    private bool _isBlocking;
    public CombatState CurrentState { get; private set; }

    private CardStats _cardStats;

    [SerializeField] private EntityUI entityUI;
    [SerializeField] private BlockerArrowHandler arrowSpawner;

    [field: SyncVar]
    public string Title { get; private set; }

    [SyncVar] private int _attack;
    [SyncVar] private int _health;

    [ClientRpc]
    public void RpcSpawnEntity(PlayerManager owner, CardInfo cardInfo, int holderNumber)
    {
        _boardManager = BoardManager.Instance;
        
        Owner = owner;
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
        CurrentState = combatState;
        if (!hasAuthority || _isAttacking) return;
        
        _canAct = true;
        entityUI.Highlight(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasAuthority || !_canAct) return;

        switch (CurrentState)
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
            entityUI.TapCreature();
        } else {
            _isAttacking = false;
            entityUI.UntapCreature();
        }

        if (isServer) _boardManager.AttackerDeclared(this, _isAttacking);
        else CmdAttackerDeclared(_isAttacking);
    }

    [Command]
    private void CmdAttackerDeclared(bool isAttacking)
    {
        _boardManager.AttackerDeclared(this, isAttacking);
    }
    
    [ClientRpc]
    public void RpcHighlightAttacker()
    {
        entityUI.HighlightAttacker(true);
    }

    private void ClickBlocker()
    {
        _isBlocking = !_isBlocking;
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
