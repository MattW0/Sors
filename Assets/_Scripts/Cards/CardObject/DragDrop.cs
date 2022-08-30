using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public class DragDrop : NetworkBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

    private GameObject _board;
    private GameObject _startParent;

    private Vector2 _startPosition;
    private Vector2 _dragOffset;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    public float returnDuration = 0.6f;
    public float timeStartReturn = 0f;

    [Header("Permissions")]
    [SerializeField] private bool isDraggable = false;
    [SerializeField] private bool isOverDropZone = false;

    private void Awake()
    {
        _board = GameObject.Find("PlayBoard");
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ChangeDragPermission(bool draggable)
    {
        isDraggable = draggable;
    }

    public void OnBeginDrag(PointerEventData eventData){
        if (!isDraggable || !hasAuthority) return;

        _startParent = transform.parent.gameObject;
        _startPosition = transform.position;
        _dragOffset = _startPosition - eventData.position;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.7f;

        transform.SetParent(_board.transform, true);
    }

    public void OnDrag(PointerEventData eventData){
        if (!isDraggable || !hasAuthority) return;

        _rectTransform.position = eventData.position + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData){
        if (!isDraggable || !hasAuthority) return;

        if (isOverDropZone) {
            _canvasGroup.alpha = 1f;
            
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager p = networkIdentity.GetComponent<PlayerManager>();
            p.PlayCard(gameObject);

            return;
        }

        // If not over dropzone, return to start position
        transform.DOMove(_startPosition, returnDuration).OnComplete(() => {
            transform.SetParent(_startParent.transform, true);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        });
    }

    // For detection if card is over dropzone
    private void OnCollisionEnter2D(Collision2D collision) => isOverDropZone = true;
    private void OnCollisionExit2D(Collision2D collision) => isOverDropZone = false;
}
