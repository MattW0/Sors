using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class CreatureEntity : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity _bze;
    [SerializeField] private CreatureEntityUI _creatureUI;
    [SerializeField] private BlockerArrowHandler _arrowHandler;

    private List<Keywords> _keywordAbilities;
    public void SetKeywords(List<Keywords> keywords) => _keywordAbilities = keywords;
    public List<Keywords> GetKeywords() => _keywordAbilities;

    [Header("State")]
    [SerializeField] private CombatState _combatState;
    [SerializeField] private bool _canAct;
    public bool CanAct { get => _canAct; private set => _canAct = value; }
    [SerializeField] private bool _isAttacking;
    public bool IsAttacking
    {
        get => _isAttacking;
        set
        {
            _isAttacking = value;
            if(_isAttacking) _creatureUI.TapCreature();
            else _creatureUI.UntapCreature(highlight: true);
        } 
    }

    private void Start(){
        // print("IsOwned: " + isOwned);

        // if(isOwned) return;
        // _creatureUI.UntapOpponentCreature();
    }

    public void CheckIfCanAct(CombatState newState)
    {
        if (!isOwned) return;
        _combatState = newState;

        if (IsAttacking) return;
        CanAct = true;
        _creatureUI.Highlight(true);
    }

    [ClientRpc]
    public void RpcIsAttacker(){
        IsAttacking = true;
        CanAct = false;
        _creatureUI.ShowAsAttacker(true);
        
        if (isOwned) return;
        _creatureUI.TapOpponentCreature();
    }

    public void OnPointerClick(PointerEventData eventData){
        if (!isOwned || !CanAct) return;

        // only in Attackers since blockerArrowHandler handles Blockers phase
        if (_combatState != CombatState.Attackers) return;
        IsAttacking = !IsAttacking;
    }

    public void LocalPlayerIsReady(){
        // to show ui change when local player presses ready button
        CanAct = false;

        if (IsAttacking) _creatureUI.ShowAsAttacker(true);
        else _creatureUI.Highlight(false);
    }

    [ClientRpc]
    public void RpcBlockerDeclared(CreatureEntity attacker){
        if (!isOwned) return;
        _arrowHandler.HandleFoundEnemyTarget(attacker);
    }
    
    [ClientRpc]
    public void RpcShowOpponentsBlockers(List<CreatureEntity> blockers){
        if (!isOwned) return;
        foreach (var blocker in blockers)
        {
            _arrowHandler.ShowOpponentBlocker(blocker.gameObject);
        }
    }

    [Server]
    public void TakesDamage(int value, bool deathtouch){
        if (deathtouch){ 
            _bze.Health = 0;
            return;
        }
        _bze.Health -= value;
    }

    [ClientRpc]
    public void RpcRetreatAttacker(){
        if (isOwned) _creatureUI.UntapCreature(highlight: false);
        else {
            _creatureUI.UntapOpponentCreature();
            _creatureUI.ShowAsAttacker(false);
        }
    }

    [ClientRpc]
    public void RpcSetCombatHighlight() => _creatureUI.CombatHighlight();
    public void SetHighlight(bool active) => _creatureUI.Highlight(active);
    public void ResetAfterCombat(){
        CanAct = false;
        IsAttacking = false;
        
        _creatureUI.ShowAsAttacker(false);
        _creatureUI.ResetHighlight();
    }

    public int GetHealth() => _bze.Health;
    public int GetAttack() => _bze.Attack;
    public string GetCardTitle() => _bze.Title;
    public PlayerManager GetOwner() => _bze.Owner;
    public PlayerManager GetOpponent() => _bze.Opponent;
}
