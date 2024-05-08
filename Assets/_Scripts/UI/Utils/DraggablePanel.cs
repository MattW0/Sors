using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IDragHandler
{
    public Canvas canvas;
    [SerializeField] private RectTransform _panelRectTransform;

    public void OnDrag(PointerEventData eventData)
    {
        // _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        _panelRectTransform.anchoredPosition += eventData.delta;

    }
}
