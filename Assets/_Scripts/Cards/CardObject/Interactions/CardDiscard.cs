using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDiscard : MonoBehaviour, IPointerDownHandler{
    private HandInteractionPanel _handInteractionPanel;
    private CardStats _stats;
    private RectTransform _rectTransform;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set{
            _isSelected = value;
            if (_isSelected) {
                _rectTransform.position += Vector3.forward;
                _handInteractionPanel.SelectCardToDiscard(gameObject);
            } else {
                _rectTransform.position += Vector3.back;
                _handInteractionPanel.DeselectCardToDiscard(gameObject);
            } 
        }
    }

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        _handInteractionPanel = HandInteractionPanel.Instance;
        _rectTransform = gameObject.GetComponent<RectTransform>();

        HandInteractionPanel.OnDiscardEnded += Reset;
    }

    public void OnPointerDown(PointerEventData eventData){
        if (!_stats.IsDiscardable) return;
        IsSelected = !_isSelected;
    }

    public void Reset() => _isSelected = false;

    private void OnDestroy()
    {
        HandInteractionPanel.OnDiscardEnded -= Reset;
    }
}
