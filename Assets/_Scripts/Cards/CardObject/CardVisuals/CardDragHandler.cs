
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class CardDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private Vector3 offset;
    private Vector2 _screenBounds;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject _cardVisualPrefab;
    [HideInInspector] public CardVisualHandler cardVisual;
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

        if (!instantiateVisual)
            return;

        cardVisual = Instantiate(_cardVisualPrefab, VisualPrefabsParent.instance.transform, false).GetComponent<CardVisualHandler>();
        cardVisual.Initialize(this);
    }

    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            var targetPosition = MouseInputHelper.GetMouseWorldPosition(_cam);
            print("targetPosition : "+ targetPosition);
            print("transform: " + transform.position);
            Vector3 direction = (targetPosition - transform.position).normalized;
            print("direction : "+ direction);

            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            print("velocity : "+ velocity);
            
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -_screenBounds.x, _screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -_screenBounds.y, _screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        print("OnBeginDrag");
        BeginDragEvent.Invoke(this);
        
        offset = MouseInputHelper.GetMouseWorldPosition(_cam) - transform.position;
        print("offset:" + offset);
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
        print("pointer enter");
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
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedSlotPosition()
    {
        if (transform.parent.CompareTag("Slot")) return 0;

        return ParentIndex() / transform.parent.parent.childCount;
    }

    private void OnDestroy()
    {
        if(cardVisual != null)
        Destroy(cardVisual.gameObject);
    }
}
