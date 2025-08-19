
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

public class CardDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    private Vector3 offset;
    private static Vector2 _screenBounds;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    private CardVisualHandler _cardVisual;
    private Camera _cam;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardDragHandler> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardDragHandler, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> EndDragEvent;
    [HideInInspector] public UnityEvent<CardDragHandler, bool> SelectEvent;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        _cam = Camera.main;
        _screenBounds = MouseInputHelper.GetScreenBounds(_cam);
    }

    internal void SetCardVisual(CardVisualHandler visual) => _cardVisual = visual;

    void LateUpdate()
    {
        if (! isDragging) return;

        var targetPosition = ClampPosition(MouseInputHelper.GetMouseWorldPosition(_cam) - offset);
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
        
        transform.Translate(velocity * Time.deltaTime);
    }

    private static Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -_screenBounds.x, _screenBounds.x);
        position.y = Mathf.Clamp(position.y, -_screenBounds.y, _screenBounds.y);
        return position;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        
        offset = MouseInputHelper.GetMouseWorldPosition(_cam) - transform.position;
        isDragging = true;
        
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += _cardVisual.transform.up * selectionOffset;
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += _cardVisual.transform.up * 50;
            else
                transform.localPosition = Vector3.zero;
        }
    }

    public void Swap(int direction) => _cardVisual.Swap(direction);

    public int ParentIndex()
    {
        // return transform.parent.GetSiblingIndex();
        var index = transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
        // print("Index: " + index);
        return index;
    }

    public int SiblingAmount() => Math.Max(1, transform.parent.parent.childCount - 1);

    private void OnDestroy()
    {
        if(_cardVisual != null)
        Destroy(_cardVisual.gameObject);
    }
}
