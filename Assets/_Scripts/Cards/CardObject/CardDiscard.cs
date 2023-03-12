using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDiscard : MonoBehaviour
{
    private HandInteractionPanel _discardPanel;
    private bool _isSelected;
    private Vector3 _startPosition;
    private CardStats _stats;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        _discardPanel = HandInteractionPanel.Instance;
        _rectTransform = gameObject.GetComponent<RectTransform>();

        HandInteractionPanel.OnDiscardEnded += Reset;
    }

    public void OnDiscardClick(){
        if (!_stats.IsDiscardable) return;
        
        if (_isSelected) {
            _isSelected = false;
            transform.position = _startPosition;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _startPosition = _rectTransform.position;
        _rectTransform.position = _startPosition + Vector3.up * 5;
        
        _discardPanel.CardToDiscardSelected(gameObject, true);
    }

    public void Reset()
    {
        _isSelected = false;
        _startPosition = Vector2.zero;
    }

    private void OnDestroy()
    {
        HandInteractionPanel.OnDiscardEnded -= Reset;
    }
}
