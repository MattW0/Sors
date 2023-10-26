using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHandler : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    private ArrowRenderer _arrowRenderer;
    
    public bool HasOrigin { get; private set; }
    public bool HasTarget { get; private set; }
    public CombatState CurrentCombatState { get; private set; }
    public void CombatStateChanged(CombatState newState){
        CurrentCombatState = newState;
        if(newState == CombatState.CleanUp) RemoveArrow(true);
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

    private void FixedUpdate()
    {
        if (!_arrowRenderer || HasTarget) return;
        _arrowRenderer.SetTarget();
    }

    public void FoundTarget(Vector3 target){
        HasTarget = true;
        _arrowRenderer.SetTarget(target);
    }

    public void RemoveArrow(bool destroyArrowObject)
    {
        if (!_arrowRenderer) return;

        HasOrigin = false;
        HasTarget = false;
        _arrowRenderer = null;
        if(destroyArrowObject) _arrowRenderer.DestroyArrow();
    }
}
