using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetArrowHandler : ArrowHandler, IPointerClickHandler
{
    private BattleZoneEntity _entity;

    private void Awake()
    {
        _entity = gameObject.GetComponent<BattleZoneEntity>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only (handled in entityUI)
        if (eventData.button == PointerEventData.InputButton.Right) return;

        // return if entity is not targetable 
        if (!_entity.IsTargetable) return;

        var player = PlayerManager.GetLocalPlayer();
        if(!player.PlayerIsChoosingTarget) return;
        
        player.PlayerChoosesEntityTarget(_entity);
    }
}
