using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTrash : MonoBehaviour
{
    private CardCollectionPanelUI _panel;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set {
            _isSelected = value;
            _trashImage.enabled = value;
        }
    }
    private CardStats _stats;
    [SerializeField] private Image _trashImage;

    private void Awake()
    {
        _stats = gameObject.GetComponent<CardStats>();
        CardCollectionPanelUI.OnTrashEnded += Reset;
    }

    public void OnTrashClick(){
        if (!_stats.IsTrashable) return;
                
        if (IsSelected) {
            IsSelected = false;
            _panel.CardTrashSelected(gameObject, false);
            return;
        }

        IsSelected = true;
        _panel.CardTrashSelected(gameObject, true);
    }

    public void Reset() => IsSelected = false;

    private void OnDestroy()
    {
        CardCollectionPanelUI.OnTrashEnded -= Reset;
    }
}
