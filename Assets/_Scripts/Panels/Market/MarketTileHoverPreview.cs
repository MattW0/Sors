using UnityEngine;
using System.Collections;

public class MarketTileHoverPreview : MonoBehaviour
{
    [SerializeField] private DetailCardPreview _detailCardPreview;
    [SerializeField] private RectTransform previewWindow;
    private Vector3 _offset = new(0, -40f, 0f);
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
        var pos = Input.mousePosition + _offset;
        // print($"Init position : {pos.x}, {pos.y}");

        var endWidth = pos.x - _viewWidth;
        if(endWidth < 0f) pos.x -= endWidth;

        var endHeight = pos.y - _viewHeight;
        if(endHeight < 0f) pos.y -= endHeight;

        // print($"Set position to : {pos.x}, {pos.y}");
        previewWindow.position = pos;
    }
}
