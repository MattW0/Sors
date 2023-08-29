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
        if (!clicker.PlayerIsChoosingTarget) return;
        
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
        var input = new Vector3(Input.mousePosition.x, 0.5f, Input.mousePosition.y);

        // Input range X: [0, 1920], Y: 0, Z: [0, 1080]
        // Arrow renderer range X: [-9.7, 9.7], Y: 0.5, Z: [-5.5, 5.5]

        // X: [0, 1920] -> X: [-9.7, 9.7]
        input.x = (input.x / 1920f) * 19.4f - 9.7f;

        // Z: [0, 1080] -> Z: [-5.5, 5.5]
        input.z = (input.z / 1080f) * 11f - 5.5f;

        // clamp
        input.x = Mathf.Clamp(input.x, -9.7f, 9.7f);
        input.z = Mathf.Clamp(input.z, -5.5f, 5.5f);

        _arrow.SetTarget(input);
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
}
