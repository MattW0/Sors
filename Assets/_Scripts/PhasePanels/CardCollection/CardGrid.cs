using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGrid : MonoBehaviour
{
    public bool updateGrid;
    [SerializeField] private RectTransform _maxViewTransform;
    [SerializeField] private Transform _cardHolder;
    private const float _panelMaxWidth = 1300f;
    private const float _defaultX = 120f;
    private const float _padding = 20f;
    private const float _defaultScale = 0.7f;
    private const float _cardWidth = 154f; // With scale factor 0.7! Default: 220f
    private float _panelWidth;

    private void Update()
    {
        if (!updateGrid) return;
        
        UpdateGrid();
        updateGrid = false;
    }

    
    internal void SetPanelWidth(int count)
    {
        _panelWidth = Mathf.Min(_panelMaxWidth, _defaultX * count + 2 * _padding);
        _maxViewTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _panelWidth);
    }

    public void AddCards(List<Transform> transforms)
    {
        foreach (var t in transforms) {
            t.SetParent(_cardHolder, false);
            t.localScale = Vector3.one * _defaultScale;
        }

        updateGrid = true;
    }

    private void UpdateGrid()
    {
        if (_cardHolder.childCount == 1) {
            _cardHolder.GetChild(0).localPosition = Vector3.zero;
            _cardHolder.GetChild(0).localEulerAngles = Vector3.zero;
            return;
        }

        var limitMin = - _panelWidth / 2f + _cardWidth / 2f + _padding;
        var limitMax = _panelWidth / 2f - _cardWidth / 2f - _padding;

        int i = 0;
        foreach (Transform child in _cardHolder) {
            var x = Mathf.Lerp(limitMin, limitMax, (float) i / (_cardHolder.childCount-1));

            child.localPosition = new Vector3(x, 0, 0);
            child.localEulerAngles = Vector3.zero;
            i++;
        }
    }
}