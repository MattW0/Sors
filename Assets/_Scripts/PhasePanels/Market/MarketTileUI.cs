using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class MarketTileUI : EntityUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private MarketTile _tile;
    [SerializeField] private Image _overlay;
    public static event Action<CardInfo> OnMarketTileInspect;
    private CancellationTokenSource _cts = new();

    private void Awake()
    {
        _tile = GetComponent<MarketTile>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            OnMarketTileInspect?.Invoke(_tile.cardInfo);
            return;
        }

        if (!_tile.Interactable) return;
        _tile.IsSelected = ! _tile.IsSelected;
    }

    private async UniTaskVoid HoverPreviewWaitTimer(CancellationToken token)
    {
        await UniTask.Delay(SorsTimings.hoverPreviewDelay, cancellationToken: token);

        token.ThrowIfCancellationRequested();
        MarketTileHoverPreview.OnHoverTile(_tile.cardInfo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO: Does not work! Check cancellation

        // print("Pointer enter");

        // Cancel previous hover preview if still running
        // _cts?.Cancel();
        // HoverPreviewWaitTimer(_cts.Token).Forget();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // print("Pointer exit");

        // _cts?.Cancel();
        // MarketTileHoverPreview.OnHoverExit();
    }

    public override void Highlight(bool enabled, Color color)
    {
        base.Highlight(enabled, color);
        _overlay.enabled = !enabled;
    }

    public void ShowAsChosen()
    {
        base.Highlight(false, SorsColors.tileSelectable);
        _overlay.enabled = true;
    }
}
