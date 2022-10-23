using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class BlockerArrowHandler : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private BattleZoneEntity entity;
    [SerializeField] private GameObject arrowPrefab;

    private Arrow _arrow;
    private bool _hasValidTarget;
    
    private void SpawnArrow()
    {
        var obj = Instantiate(arrowPrefab, transform.localPosition, Quaternion.identity);
        _arrow = obj.GetComponent<Arrow>();
        _arrow.SetAnchor(entity.transform.position);
    }

    public void DestroyArrow()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (entity.CurrentState != CombatState.Blockers) return;
        
        if (!_hasValidTarget){
            SpawnArrow();
        }

        if (_hasValidTarget)
        {
            var targetObject = eventData.pointerPress;
            entity.target = targetObject.GetComponent<BattleZoneEntity>();
        }
    }
}
