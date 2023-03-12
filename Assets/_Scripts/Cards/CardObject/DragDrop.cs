using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public class DragDrop : MonoBehaviour, IPointerDownHandler { /*, IPointerUpHandler, IDragHandler {*/

    private GameObject _board;
    private Transform _startTransform;
    private Transform _endTransform;

    private Vector3 _startPosition;
    private Vector3 _dragOffset;
    private RectTransform _transform;
    private CanvasGroup _canvasGroup;

    public float returnDuration = 0.3f;

    private CardStats _cardStats;
    private bool _isOverDropZone;
    private int _collisionCount;

    private void Awake()
    {
        _board = GameObject.Find("PlayBoard");
        _transform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        _cardStats = gameObject.GetComponent<CardStats>();
    }

    // TODO: Make entity spawning with stack
    // OnpointerDown -> add to first free slot
    
    public void OnPointerDown(PointerEventData eventData){
        if (!_cardStats.IsDeployable) return;

        _startTransform = _transform.parent.gameObject.transform;
        // _startPosition = _transform.position;
        // _dragOffset = _startPosition - new Vector3(eventData.position.x, eventData.position.y, 0);
        
        // _canvasGroup.blocksRaycasts = false;
        // _canvasGroup.alpha = 0.7f;

        transform.SetParent(_board.transform, true);
    }

    // public void OnDrag(PointerEventData eventData){
    //     if (!_cardStats.IsDeployable) return;

    //     var tempVector = new Vector3(eventData.position.x, eventData.position.y, 0);
    //     tempVector += _dragOffset;
    //     tempVector.z = 0;
    //     _transform.position = tempVector;
    // }

    // public void OnPointerUp(PointerEventData eventData){
    //     if (!_cardStats.IsDeployable) return;

    //     // If not over dropzone, return to start position
    //     if (!_isOverDropZone){
    //         _transform.DOMove(_startPosition, returnDuration).OnComplete(() =>
    //         {
    //             _transform.SetParent(_startTransform, true);
    //         });
    //     }
        
    //     _canvasGroup.blocksRaycasts = true;
    //     _canvasGroup.alpha = 1f;
    // }

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
