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
    private Vector3 _offset = new Vector3(960f, 540f, 0f);

    private void Awake()
    {
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
    }

    [ClientRpc]
    private void RpcCombatStateChanged(CombatState newState)
    {
        _currentState = newState;
        if(_currentState == CombatState.CleanUp) {
            _hasTarget = false;
        }
    }
    
    private ArrowRenderer SpawnArrow()
    {
        var obj = Instantiate(arrowPrefab);
        var arrow = obj.GetComponent<ArrowRenderer>();
        arrow.SetOrigin(creature.transform.position);

        return arrow;
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
            _arrow = SpawnArrow();
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
        var arrow = SpawnArrow();
        arrow.SetTarget(blocker.transform.position);
    }

    private void FixedUpdate(){
        if (!_arrow || _hasTarget) return;
        
        // TODO: Arrow position is something in the range of [(-10, -7), (10, 7)] (x, z)
        // Find exact values, and offset to middle of screen (0, 0)
        // Normalize mouse position to this range and it will work
        // y value should be around 1.5f
        var v = Input.mousePosition;
        print(v);
        var v2 = new Vector3(v.x, 1.5f, v.y);
        v2 = v2 - _offset;
        v2 = v2 / 500f;
        print(v2);
        _arrow.SetTarget(v2);
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
}
