using System;
using UnityEngine;
using Mirror;

public class BlockerArrowHandler : NetworkBehaviour
{
    [SerializeField] private BattleZoneEntity entity;
    [SerializeField] private GameObject arrowPrefab;
    
    private ArrowRenderer _arrow;
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
        if(_currentState != CombatState.CleanUp) {
            _hasTarget = false;
        }
    }
    
    private void SpawnArrow()
    {
        var obj = Instantiate(arrowPrefab);
        _arrow = obj.GetComponent<ArrowRenderer>();
        _arrow.SetOrigin(entity.transform.position);
        // _arrow.SetAnchor(entity.transform.position);
    }

    public void OnClickEntity()
    {
        // return if not in Blockers Phase
        if (_currentState != CombatState.Blockers) return;

        if (entity.isOwned) HandleClickedMyCreature();
        else HandleClickedOpponentCreature();
    }
    
    private void HandleClickedMyCreature(){
        
        if (!entity.CanAct || entity.IsAttacking || _hasTarget) return;
        
        if (!_arrow) {
            SpawnArrow();
            entity.Owner.PlayerChoosesBlocker(entity);
            return;
        }
        
        entity.Owner.PlayerRemovesBlocker(entity);
        Destroy(_arrow.gameObject);
        _arrow = null;
    }

    private void HandleClickedOpponentCreature()
    {
        if (!entity.IsAttacking) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingBlockers) return;
        
        clicker.PlayerChoosesAttackerToBlock(entity);
    }

    public void HandleFoundEnemyTarget(BattleZoneEntity target)
    {
        _hasTarget = true;
        _arrow.SetTarget(target.transform.position);
    }
    
    public void ShowOpponentBlocker(GameObject blocker)
    {
        SpawnArrow();
        _arrow.SetTarget(blocker.transform.position);
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
}
