using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class BlockerArrowHandler : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private CreatureEntity creature;
    [SerializeField] private GameObject arrowPrefab;
    private GameObject _arrowObject;
    private ArrowRenderer _arrow;
    private CombatState _currentState;
    private bool _hasTarget;
    private Vector3 _offset = new Vector3(960f, 540f, 0f);

    private void Awake(){
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
    }

    [ClientRpc]
    private void RpcCombatStateChanged(CombatState newState){
        _currentState = newState;
        if(_currentState == CombatState.CleanUp) {
            _hasTarget = false;
        }
    }
    
    private void SpawnArrow(){
        _arrowObject = Instantiate(arrowPrefab);
        _arrow = _arrowObject.GetComponent<ArrowRenderer>();

        var origin = creature.transform.position;
        origin.y = 0.5f;
        _arrow.SetOrigin(origin);
    }

    public void OnPointerClick(PointerEventData eventData){
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

    private void HandleClickedOpponentCreature(){
        if (!creature.IsAttacking) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingTarget) return;
        
        clicker.PlayerChoosesAttackerToBlock(creature);
    }

    public void HandleFoundEnemyTarget(CreatureEntity target){
        _hasTarget = true;
        _arrow.SetTarget(target.transform.position);
    }
    
    public void ShowOpponentBlocker(GameObject blocker){
        SpawnArrow();
        _arrow.SetTarget(blocker.transform.position);
    }

    private void FixedUpdate(){
        if (!_arrow || _hasTarget) return;
        _arrow.SetTarget();
    }

    public void DestroyArrow(){
        if (!_arrowObject) return;

        Destroy(_arrowObject);
        _arrowObject = null;
        _arrow = null;
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
}
