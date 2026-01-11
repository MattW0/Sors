using UnityEngine;
using System.Collections;

public class MarketTileHoverPreview : MonoBehaviour
{
    [SerializeField] private CardHoverView _hoverView;
    [SerializeField] private DetailCardPreview _detailCardPreview;
    private WaitForSeconds _wait = new(SorsTimings.hoverPreviewDelay);
    
    private Coroutine _hoverCoroutine;
    private Coroutine _exitCoroutine;
    private bool _isVisible;

    private void OnEnable()
    {
        MarketTileUI.OnHoverTile += HoverStart;
        MarketTileUI.OnHoverExit += HoverExit;
    }

    private void OnDisable()
    {
        MarketTileUI.OnHoverTile -= HoverStart;
        MarketTileUI.OnHoverExit -= HoverExit;
    }

    private void Start()
    {
        HideImmediate();
        _detailCardPreview.HideAll();
    }
    
    private void HoverStart(CardInfo card)
    {
        // Cancel exit if we're moving to another card
        if (_exitCoroutine != null) {
            StopCoroutine(_exitCoroutine);
            _exitCoroutine = null;
        }

        if (_isVisible) {
            _hoverView.Show(card);
            return;
        }

        if (_hoverCoroutine != null)
            StopCoroutine(_hoverCoroutine);

        _hoverCoroutine = StartCoroutine(HoverDelay(card));
    }

    private void HoverExit()
    {
        if (_hoverCoroutine != null) {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }

        if (_exitCoroutine != null)
            StopCoroutine(_exitCoroutine);

        _exitCoroutine = StartCoroutine(ExitDelay());
    }

    private IEnumerator HoverDelay(CardInfo card)
    {
        yield return _wait;

        _isVisible = true;
        _hoverView.Show(card);
    }

    private IEnumerator ExitDelay()
    {
        yield return _wait;

        _isVisible = false;
        _hoverView.Hide();
    }

    private void HideImmediate()
    {
        _isVisible = false;

        if (_hoverCoroutine != null) StopCoroutine(_hoverCoroutine);
        if (_exitCoroutine != null) StopCoroutine(_exitCoroutine);

        _hoverView.Hide();
    }
}
