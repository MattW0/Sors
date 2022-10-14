using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    private GameObject _board;
    private Transform _startTransform;
    private Transform _endTransform;

    private Vector2 _startPosition;
    private Vector2 _dragOffset;
    private Transform _transform;
    private CanvasGroup _canvasGroup;

    public float returnDuration = 0.3f;

    private CardStats _cardStats;
    private bool _isOverDropZone;
    private int _collisionCount;

    private void Awake()
    {
        _board = GameObject.Find("PlayBoard");
        _transform = GetComponent<Transform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        _cardStats = gameObject.GetComponent<CardStats>();
    }

    public void OnPointerDown(PointerEventData eventData){
        if (!_cardStats.IsDeployable) return;

        var cardTransform = transform;
        _startTransform = cardTransform.parent.gameObject.transform;
        _startPosition = cardTransform.position;
        _dragOffset = _startPosition - eventData.position;
        
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.7f;

        transform.SetParent(_board.transform, true);
    }

    public void OnDrag(PointerEventData eventData){
        if (!_cardStats.IsDeployable) return;

        _transform.position = eventData.position + _dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData){
        if (!_cardStats.IsDeployable) return;

        // If not over dropzone, return to start position
        if (!_isOverDropZone){
            _transform.DOMove(_startPosition, returnDuration).OnComplete(() =>
            {
                _transform.SetParent(_startTransform, true);
            });
        }
        
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        if (_isOverDropZone) {
            _cardStats.IsDeployable = false;
        }
    }

    // Collision detection with dropzone collider
    private void OnCollisionEnter2D()
    {
        _collisionCount++;
        _isOverDropZone = true;
    }
    private void OnCollisionExit2D()
    {
        _collisionCount--;

        if (_collisionCount > 0) return;
        _isOverDropZone = false;
    }
}
