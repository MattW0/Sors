using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDiscard : MonoBehaviour
{
    private HandInteractionPanel _discardPanel;
    private bool _isSelected;
    private Vector2 _startPosition;
    private CardStats _stats;

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        _discardPanel = HandInteractionPanel.Instance;

        HandInteractionPanel.OnDiscardEnded += Reset;
    }

    public void OnDiscardClick(){
        if (!_stats.IsDiscardable) return;
        
        var trans = transform;
        
        if (_isSelected) {
            _isSelected = false;
            transform.position = _startPosition;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _startPosition = trans.position;
        trans.position = _startPosition + Vector2.up * 10;
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
