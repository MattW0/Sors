using UnityEngine;
using System.Collections;

public class MarketTileHoverPreview : MonoBehaviour
{
    [SerializeField] private DetailCardPreview _detailCardPreview;
    [SerializeField] private RectTransform previewWindow;
    private Vector3 _offset = new(10f, -10f, 0f);
    private float _viewHeight;
    private float _viewWidth;
    private WaitForSeconds _wait = new(SorsTimings.hoverPreviewDelay); 

    private void Awake()
    {
        _viewHeight = previewWindow.rect.height;
        _viewWidth = previewWindow.rect.width;
    }

    private void OnEnable()
    {
        MarketTileUI.OnHoverTile += HoverStart;
        MarketTileUI.OnHoverExit += HidePreview;
    }

    private void OnDisable()
    {
        MarketTileUI.OnHoverTile -= HoverStart;
        MarketTileUI.OnHoverExit -= HidePreview;
    }

    private void Start()
    {
        HidePreview();
        _detailCardPreview.HideAll();
    }

    //TODO: Make sure the whole window is within screen bounds
    private void HoverStart(CardInfo cardInfo)
    {
        HidePreview();
        StartCoroutine(HoverDelay(cardInfo));
    }

    private IEnumerator HoverDelay(CardInfo card)
    {
        yield return _wait;

        SetViewPosition();
        _detailCardPreview.ShowPreview(card, card.type != CardType.Money);
    }


    private void HidePreview()
    {   
        StopAllCoroutines();
        _detailCardPreview.HideAll();
    }

    private void SetViewPosition()
    {
        var anchor = Input.mousePosition + _offset;

        var endWidth = anchor.x + _viewWidth;
        var endHeight = anchor.y - _viewHeight;
        if(endWidth > Screen.width) anchor.x -= endWidth - Screen.width;
        if(endHeight < 0f) anchor.y -= endHeight;

        previewWindow.position = anchor;
    }
}
