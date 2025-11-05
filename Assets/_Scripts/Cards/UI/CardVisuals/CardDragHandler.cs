
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using Mirror;

public class CardDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public CardStats Stats;
    private Image imageComponent;
    private Vector3 offset;
    private static Vector2 _screenBounds;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Visual")]
    public CardVisualHandler cardVisual;

    [Header("States")]
    public bool Draggable { get; set; }
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardDragHandler> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardDragHandler, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardDragHandler> EndDragEvent;
    public static event Action<CardInfo> OnInspect;
    public static event Action<CardStats> OnCardClicked;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        _screenBounds = MouseInputHelper.GetScreenBounds();
    }

    internal void Initialize(CardVisualHandler cardVisual, CardStats stats, CardPileCurveParameters curve)
    {
        // print("Initialize carddraghandler on " + stats.cardInfo.title); 
        Stats = stats;
        cardVisual.Initialize(this, stats.transform, curve);
    }

    void LateUpdate()
    {
        if (! isDragging) return;

        var targetPosition = ClampPosition(MouseInputHelper.GetMouseWorldPosition() - offset);
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
        if (eventData.button == PointerEventData.InputButton.Right)
            OnInspect?.Invoke(Stats.cardInfo);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) return;
        if (! Stats.IsInteractable || wasDragged) return;
        
        OnCardClicked?.Invoke(Stats);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!Draggable) return; 

        BeginDragEvent.Invoke(this);
        
        offset = MouseInputHelper.GetMouseWorldPosition() - transform.position;
        isDragging = true;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData) { }

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

    internal void ResetPosition()
    {
        transform.localPosition = Vector3.zero;
        cardVisual.Reset();
    }
}
