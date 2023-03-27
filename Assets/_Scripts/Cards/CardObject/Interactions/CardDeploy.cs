using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDeploy : MonoBehaviour, IPointerDownHandler {

    private CardCollectionPanelUI _panel;
    private CardStats _stats;
    private RectTransform _rectTransform;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set{
            _isSelected = value;
            if (_isSelected) {
                _rectTransform.position += Vector3.forward;
                _panel.SelectCardToDeploy(gameObject);
            } else {
                _rectTransform.position += Vector3.back;
                _panel.DeselectCardToDeploy(gameObject);
            } 
        }
    }

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        _panel = CardCollectionPanelUI.Instance;
        _rectTransform = gameObject.GetComponent<RectTransform>();

        CardCollectionPanelUI.OnDeployEnded += Reset;
    }
    
    public void OnPointerDown(PointerEventData eventData){
        if (!_stats.IsDeployable) return;
        IsSelected = !_isSelected;
    }

    public void Reset() => _isSelected = false;

    private void OnDestroy()
    {
        CardCollectionPanelUI.OnDeployEnded -= Reset;
    }
}
