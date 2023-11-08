using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHandler : MonoBehaviour
{
    // TODO: Check if it makes more sense to init localPlayer instead of checking
    // on each click, who the clicker is (Attacker and Blocker Handlers)
    // private PlayerManager _localPlayer;
    [SerializeField] private GameObject arrowPrefab;
    private ArrowRenderer _arrowRenderer;
    public bool HasOrigin { get; private set; }
    public bool HasTarget { get; private set; }
    public CombatState CurrentCombatState { get; private set; }
    public void CombatStateChanged(CombatState newState){
        CurrentCombatState = newState;
        if(newState == CombatState.CleanUp) RemoveArrow(true);
    }
    
    private void FixedUpdate()
    {
        if (!_arrowRenderer || HasTarget) return;
        _arrowRenderer.SetTarget();
    }

    public void SpawnArrow()
    {
        var arrowObject = Instantiate(arrowPrefab);
        _arrowRenderer = arrowObject.GetComponent<ArrowRenderer>();

        var origin = transform.position;
        origin.y = 0.5f;
        _arrowRenderer.SetOrigin(origin);

        HasOrigin = true;
    }


    public void HandleFoundTarget(Transform target)
    {
        if(!HasOrigin) SpawnArrow();

        HasTarget = true;
        _arrowRenderer.SetTarget(target.position);
    }

    public void RemoveArrow(bool destroyArrowObject)
    {
        if (!_arrowRenderer) return;

        HasOrigin = false;
        HasTarget = false;
        
        if(destroyArrowObject) _arrowRenderer.DestroyArrow();
        _arrowRenderer = null;
    }
}
