using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayZoneCardHolder : MonoBehaviour
{
    [SerializeField] private Image highlight;
    private void Awake()
    {
        highlight.enabled = false;
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
