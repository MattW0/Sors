using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class BlockerArrowHandler : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private CreatureEntity creature;
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
        _arrow.SetOrigin(creature.transform.position);
        // _arrow.SetAnchor(creature.transform.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // return if not in Blockers Phase
        if (_currentState != CombatState.Blockers) return;

        if (creature.isOwned) HandleClickedMyCreature();
        else HandleClickedOpponentCreature();
    }
    
    private void HandleClickedMyCreature(){
        
        if (!creature.CanAct || creature.IsAttacking || _hasTarget) return;
        
        if (!_arrow) {
            SpawnArrow();
            creature.GetOwner().PlayerChoosesBlocker(creature);
            return;
        }
        
        creature.GetOwner().PlayerRemovesBlocker(creature);
        Destroy(_arrow.gameObject);
        _arrow = null;
    }

    private void HandleClickedOpponentCreature()
    {
        if (!creature.IsAttacking) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingBlockers) return;
        
        clicker.PlayerChoosesAttackerToBlock(creature);
    }

    public void HandleFoundEnemyTarget(CreatureEntity target)
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
