using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class EntityClickHandler : MonoBehaviour, IPointerClickHandler
{
    private BattleZoneEntity _entity;
    public static event Action<BattleZoneEntity> OnEntityClicked;
    public static event Action<CardInfo> OnInspect;

    private void Awake()
    {
        _entity = gameObject.GetComponent<BattleZoneEntity>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to inspect card, left click to take action
        if (eventData.button == PointerEventData.InputButton.Right) OnInspect?.Invoke(_entity.CardInfo);
        else OnEntityClicked?.Invoke(_entity);
    }
}
