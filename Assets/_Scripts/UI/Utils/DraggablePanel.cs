using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform _panelRectTransform;
    private Vector3 _position;

    public void OnDrag(PointerEventData eventData)
    {
        _panelRectTransform.anchoredPosition += eventData.delta;
        _position = _panelRectTransform.anchoredPosition;
    }

    private void OnEnable() {
        _panelRectTransform.anchoredPosition = _position;
    }
}
