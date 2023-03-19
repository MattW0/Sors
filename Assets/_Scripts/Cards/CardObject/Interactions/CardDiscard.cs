using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDiscard : MonoBehaviour
{
    private HandInteractionPanel _handInteractionPanel;
    private CardStats _stats;
    private RectTransform _rectTransform;
    private bool _isSelected;
    

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        _handInteractionPanel = HandInteractionPanel.Instance;
        _rectTransform = gameObject.GetComponent<RectTransform>();

        HandInteractionPanel.OnDiscardEnded += Reset;
    }

    public void OnDiscardClick(){
        if (!_stats.IsDiscardable) return;
        
        if (_isSelected) {
            _isSelected = false;
            _handInteractionPanel.CardToDiscardSelected(gameObject, false);
            _rectTransform.position += Vector3.back * 3;
            return;
        }

        _isSelected = true;
        _handInteractionPanel.CardToDiscardSelected(gameObject, true);
        _rectTransform.position += Vector3.forward * 3;
    }

    public void Reset() => _isSelected = false;

    private void OnDestroy()
    {
        HandInteractionPanel.OnDiscardEnded -= Reset;
    }
}
