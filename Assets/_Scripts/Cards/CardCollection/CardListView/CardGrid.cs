using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CardGrid : MonoBehaviour
{
    public bool updateGrid;
    [SerializeField] private RectTransform _maxViewTransform;
    [SerializeField] private Transform _parentTransform;
    public Vector2 itemDimensions = new(220, 360);
    public float itemScaleFactor = 0.7f;
    public Vector2 padding = new(20f, 10f);
    public float gap = 10f;
    public const float PANEL_MAX_WIDTH = 1300f;
    private const float HEADER_HEIGHT = 40f;
    private float _panelWidth;

    private void Update()
    {
        if (!updateGrid) return;
        
        UpdateGrid();
        updateGrid = false;
    }

    
    internal void SetPanelDimension(int count)
    {
        var minWidth = itemDimensions.x*count*itemScaleFactor + gap*(count-1) + 2*padding.x;
        _panelWidth = Mathf.Min(PANEL_MAX_WIDTH, minWidth);
        _maxViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _panelWidth);

        var height = itemDimensions.y*itemScaleFactor + 2*padding.y + HEADER_HEIGHT;
        _maxViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void Add(List<Transform> transforms)
    {
        foreach (var t in transforms) {
            t.SetParent(_parentTransform, false);
            t.localScale = Vector3.one * itemScaleFactor;
        }

        updateGrid = true;
    }

    public void Add(Transform transform)
    {
        transform.SetParent(_parentTransform, false);
        transform.localScale = Vector3.one * itemScaleFactor;
        updateGrid = true;
    }

    private void UpdateGrid()
    {
        if (_parentTransform.childCount == 1) {
            _parentTransform.GetChild(0).localPosition = Vector3.zero;
            return;
        }

        var limit = _panelWidth / 2f - itemDimensions.x*itemScaleFactor / 2f - padding.x;

        int i = 0;
        foreach (Transform child in _parentTransform) {
            var x = Mathf.Lerp(-limit, limit, (float) i / (_parentTransform.childCount-1));

            child.localPosition = new Vector3(x, 0, 0);
            child.localEulerAngles = Vector3.zero;
            i++;
        }
    }

    private void OnEnable()
    {
        SetPanelDimension(_parentTransform.childCount);
        updateGrid = true;
    }
}