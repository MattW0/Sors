using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetArrowHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity entity;
    [SerializeField] private GameObject arrowPrefab;
    private GameObject _arrowObject;
    private ArrowRenderer _arrow;
    private bool _hasTarget;

    public void SpawnArrow()
    {
        _arrowObject = Instantiate(arrowPrefab);
        _arrow = _arrowObject.GetComponent<ArrowRenderer>();

        var origin = entity.transform.position;
        origin.y = 0.5f;
        _arrow.SetOrigin(origin);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // return if entity is not targetable 
        if (!entity.IsTargetable) return;

        var player = PlayerManager.GetLocalPlayer();
        if(!player.PlayerIsChoosingTarget) return;
        
        player.PlayerChoosesEntityTarget(entity);
    }

    public void HandleFoundTarget(BattleZoneEntity target)
    {
        if(!_arrow) SpawnArrow();

        _hasTarget = true;
        _arrow.SetTarget(target.transform.position);
    }

    private void FixedUpdate()
    {
        if (!_arrow || _hasTarget) return;
        _arrow.SetTarget();
    }

    public void RemoveArrow(bool destroyArrowObject)
    {
        if (!_arrowObject) return;

        if(destroyArrowObject) _arrow.DestroyArrow();
        _arrowObject = null;
        _arrow = null;
    }
}
