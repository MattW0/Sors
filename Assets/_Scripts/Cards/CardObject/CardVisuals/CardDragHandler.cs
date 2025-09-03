
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

public class CardDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
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
    public CardVisualHandler cardVisual;
    private Camera _cam;

    [Header("States")]
    [SerializeField] private CardClickHandler _cardClickListener;
    public bool isHovering;
    public bool isDragging;
    private bool isDraggable;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardDragHandler> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardDragHandler, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> EndDragEvent;
    [HideInInspector] public event Action<bool> OnSelect;
    public event Action OnInspect;


    void Start()
    {
        imageComponent = GetComponent<Image>();
        _cam = Camera.main;
        _screenBounds = MouseInputHelper.GetScreenBounds(_cam);
    }

    internal void Initialize(CardClickHandler card, CardVisualHandler cardVisual)
    {
        _cardClickListener = card;
        _cardClickListener.AddObserver(this);

        cardVisual.Initialize(this, card.transform);
    }

    internal void MakeStatic()
    {
        selected = false;
        isDraggable = false;
    }

    internal void MakeSortable()
    {
        selected = false;
        isDraggable = true;
    }

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

    public void OnDrag(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!isDraggable) return; 

        BeginDragEvent.Invoke(this);
        
        offset = MouseInputHelper.GetMouseWorldPosition(_cam) - transform.position;
        isDragging = true;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
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
        if(!isDraggable) return; 

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {

        }
        pointerUpTime = Time.time;
        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        OnSelect?.Invoke(selected);

        if (selected)
            transform.localPosition += cardVisual.transform.up * selectionOffset;
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += cardVisual.transform.up * 50;
            else
                transform.localPosition = Vector3.zero;
        }
    }

    // public void Swap(int direction) => cardVisual.Swap(direction);

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
        if(cardVisual != null)
        Destroy(cardVisual.gameObject);
    }
}
