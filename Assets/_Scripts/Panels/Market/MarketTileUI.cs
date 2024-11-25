using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;
using Sirenix.OdinInspector;

public class MarketTileUI : EntityUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private MarketTile _tile;
    [SerializeField] private Image _overlay;
    public static event Action<CardInfo> OnInspect;
    public static Action<CardInfo> OnHoverTile;
    public static Action OnHoverExit;

    private void Awake()
    {
        _tile = GetComponent<MarketTile>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            OnInspect?.Invoke(_tile.cardInfo);
            return;
        }

        if (!_tile.Interactable) return;
        _tile.IsSelected = ! _tile.IsSelected;
    }

    public void OnPointerEnter(PointerEventData eventData) => OnHoverTile?.Invoke(_tile.cardInfo);
    public void OnPointerExit(PointerEventData eventData) => OnHoverExit?.Invoke();

    public override void Highlight(HighlightType type)
    {
        base.Highlight(type);
        _overlay.enabled = type == HighlightType.None;
    }

    public void ShowAsChosen()
    {
        base.Highlight(HighlightType.None);
        _overlay.enabled = true;
    }
}
