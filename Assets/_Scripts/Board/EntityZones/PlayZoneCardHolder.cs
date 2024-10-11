using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PlayZoneCardHolder : MonoBehaviour
{
    [SerializeField] private Image highlight;
    [SerializeField] private Transform entityParent;
    private BattleZoneEntity _entity;

    private void Awake()
    {
        highlight.enabled = false;
    }

    public void EntityEnters(BattleZoneEntity entity)
    {
        _entity = entity;
        entity.transform.DOMove(entityParent.position, SorsTimings.cardMoveTime)
        .SetEase(Ease.InOutCubic)
        .OnComplete(() => {
            entity.transform.SetParent(entityParent, true);
            entity.transform.localScale = Vector3.one;
        });
    }

    public void EntityLeaves()
    {
        // TODO: Add animation
    }

    public void SetHighlight()
    {
        highlight.enabled = true;
    }

    public void ResetHighlight()
    {
        highlight.enabled = false;
    }
}
