using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetArrowHandler : ArrowHandler, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity _entity;

    public void OnPointerClick(PointerEventData eventData)
    {
        // return if entity is not targetable 
        if (!_entity.IsTargetable) return;

        var player = PlayerManager.GetLocalPlayer();
        if(!player.PlayerIsChoosingTarget) return;
        
        player.PlayerChoosesEntityTarget(_entity);
    }

    public void HandleFoundTarget(BattleZoneEntity target)
    {
        if(!HasOrigin) SpawnArrow();

        FoundTarget(target.transform.position);
    }
}
