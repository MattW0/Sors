using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mirror;

public class PlayZoneCardHolder : MonoBehaviour
{
    [SerializeField] private Image highlight;
    [SerializeField] private Transform entityParent;
    public bool IsOccupied { get; internal set; }

    private void Awake()
    {
        highlight.enabled = false;
    }

    public void EntityEnters(Transform t)
    {
        t.DOMove(entityParent.position, SorsTimings.cardMoveTime)
        .SetEase(Ease.InOutCubic)
        .OnComplete(() => {
            t.SetParent(entityParent, true);
            t.localScale = Vector3.one;
        });
    }

    public void EntityLeaves()
    {
        // TODO: Add animation
        IsOccupied = false;
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
