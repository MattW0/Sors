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
        if (!entity.Targetable) return;

        var player = PlayerManager.GetLocalPlayer();
        if(!player.PlayerIsChoosingTarget) return;
        
        player.PlayerChoosesEntityTarget(entity);
    }

    public void HandleFoundTarget(BattleZoneEntity target){
        _hasTarget = true;
        _arrow.SetTarget(target.transform.position);
    }

    public void HandleOpponentFoundTarget(BattleZoneEntity target){
        SpawnArrow();
        _hasTarget = true;
        _arrow.SetTarget(target.transform.position);

        // TODO: Expand this for multiple targets -> need to rebuild _arrow and _arrowObject to lists
        // foreach(var target in targets){
        // }
    }

    private void FixedUpdate(){
        if (!_arrow || _hasTarget) return;
        _arrow.SetTarget();
    }

    public void RemoveArrow(){
        if (!_arrowObject) return;

        _arrowObject = null;
        _arrow = null;
    }
}
