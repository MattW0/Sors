using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UI;

public class BlockerArrowHandler : NetworkBehaviour
{
    [SerializeField] private BattleZoneEntity entity;
    [SerializeField] private GameObject arrowPrefab;

    private Arrow _arrow;
    private CombatState _currentState;
    private bool _hasTarget;

    private void Awake()
    {
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
    }

    [ClientRpc]
    private void RpcCombatStateChanged(CombatState newState)
    {
        _currentState = newState;
    }
    
    private void SpawnArrow()
    {
        var obj = Instantiate(arrowPrefab, transform.localPosition, Quaternion.identity);
        _arrow = obj.GetComponent<Arrow>();
        _arrow.SetAnchor(entity.transform.position);
    }

    public void OnClickEntity()
    {
        // return if not in Blockers Phase
        if (_currentState != CombatState.Blockers) return;

        if (entity.hasAuthority) HandleClickedMyCreature();
        else HandleClickedOpponentCreature();
    }
    
    private void HandleClickedMyCreature(){
        
        if (entity.IsAttacking || _hasTarget) return;
        
        if (!_arrow) {
            SpawnArrow();
            entity.Owner.PlayerChoosesBlocker(entity);
            return;
        }
        
        entity.Owner.PlayerRemovesBlocker(entity);
        _arrow.DestroyArrow();
        _arrow = null;
    }

    private void HandleClickedOpponentCreature()
    {
        if (isServer) {
            if (!entity.ServerIsAttacker()) return;
        }
        else if (!entity.IsAttacking) return;

        var clicker = PlayerManager.GetPlayerManager();
        if (!clicker.PlayerIsChoosingBlockers) return;
        
        clicker.PlayerChoosesAttackerToBlock(entity);
    }

    public void HandleFoundEnemyTarget(BattleZoneEntity target)
    {
        print("Blocking attacker " + target.Title + " with " + entity.Title);
        _hasTarget = true;
        _arrow.FoundTarget(target.transform.position);
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
    }
}
