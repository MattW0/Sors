using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform _panelRectTransform;
    private Vector3 _position;
    private Vector2 _minBounds;
    private Vector2 _maxBounds;

    public void OnDrag(PointerEventData eventData)
    {
        _panelRectTransform.anchoredPosition += eventData.delta;
        ClampToScreen();
        _position = _panelRectTransform.anchoredPosition;
    }

    private void OnEnable() {
        _panelRectTransform.anchoredPosition = _position;
        ClampToScreen();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Recalculate if parent or panel size changes (i.e. cards are added)
        CacheBounds();
    }

    private void CacheBounds()
    {
        Vector2 screenSize = new(Screen.width, Screen.height);
        Vector2 panelSize  = _panelRectTransform.rect.size;

        // Work in canvas coordinates
        _minBounds = (screenSize - panelSize) * -0.5f;
        _maxBounds = (screenSize - panelSize) *  0.5f;
    }

    private void ClampToScreen()
    {
        Vector2 pos = _panelRectTransform.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, _minBounds.y, _maxBounds.y);

        _panelRectTransform.anchoredPosition = pos;
    }
}
