using UnityEngine;
using DG.Tweening;
using System;

public class CardVisualHandler : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    public CardDragHandler _dragHandler;
    private Transform _slotTransform;
    private Canvas _canvas;
    private Camera _cam; 

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;
    [SerializeField] private float _rotationSpeed = 10;
    private Vector3 _movementDelta;
    private Vector3 _rotationDelta;
    private int _savedIndex;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float autoTiltAmount = 5;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Select and hover Parameters")]
    [SerializeField] private bool shakeAnimations = true;
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private float selectionOffset = 30f;
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Curve")]
    private CardPileCurveParameters _curve;

    private Vector3 _lastPosition;
    private float _yPositionOffset;
    private float _zRotationOffset;
    private bool _initalized = false;
    private bool _isSelected;

    private void Start()
    {
        _cam = Camera.main;
    }

    public void Initialize(CardDragHandler dragHandler, Transform cardTransform, CardPileCurveParameters curve, int index = 0)
    {
        //Declarations
        _dragHandler = dragHandler;
        dragHandler.cardVisual = this;

        _slotTransform = dragHandler.transform;
        _lastPosition = transform.position;

        cardTransform.SetParent(tiltParent, false);
        cardTransform.localPosition = Vector3.zero;

        _canvas = GetComponent<Canvas>();
        _curve = curve;

        //Event Listening
        _dragHandler.PointerEnterEvent.AddListener(PointerEnter);
        _dragHandler.PointerExitEvent.AddListener(PointerExit);
        _dragHandler.BeginDragEvent.AddListener(BeginDrag);
        _dragHandler.EndDragEvent.AddListener(EndDrag);
        
        CardSelectionHandler.OnCardSelection += OnSelect;

        //Initialization
        _initalized = true;
    }

    void LateUpdate()
    {
        if (!_initalized || _dragHandler == null) return;

        // Apply curve y position
        HandPositioning();

        // Apply static rotation or hover tilt
        CardTilt();

        if (! _dragHandler.isDragging ) return;
        FollowDrag();
    }

    private void HandPositioning()
    {
        var normalPosition = NormalizedSlotPosition();

        _zRotationOffset = _curve.rotation.Evaluate(normalPosition)  * _dragHandler.SiblingAmount();

        _yPositionOffset = _curve.positioning.Evaluate(normalPosition) * _curve.positioningInfluence;
        if (_dragHandler.SiblingAmount() < 5) _yPositionOffset = 0;

        if (_dragHandler.isDragging) return;
        transform.localPosition = new Vector3(_slotTransform.position.x, _yPositionOffset, _slotTransform.position.z);
    }

    private void CardTilt()
    {
        if (_dragHandler.isDragging) return;

        _savedIndex = _dragHandler.isDragging ? _savedIndex : _dragHandler.ParentIndex();
        float sine = Mathf.Sin(Time.time + _savedIndex) * (_dragHandler.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + _savedIndex) * (_dragHandler.isHovering ? .2f : 1);

        Vector3 offset = transform.position - MouseInputHelper.GetMouseWorldPosition(_cam);
        float tiltX = _dragHandler.isHovering ? (offset.y * -1 * manualTiltAmount) : 0;
        float tiltY = _dragHandler.isHovering ? (offset.x * manualTiltAmount) : 0;
        float tiltZ = _zRotationOffset * _curve.rotationInfluence;

        // Target tilt
        Quaternion targetRotation = Quaternion.Euler(
            tiltX + (sine * autoTiltAmount), 
            tiltY + (cosine * autoTiltAmount), 
            tiltZ
        );

        // Smooth rotation
        tiltParent.localRotation = Quaternion.Lerp(
            tiltParent.localRotation,
            targetRotation,
            tiltSpeed * Time.deltaTime
        );
    }

    private void FollowDrag()
    {
        Vector3 movement = _slotTransform.position - _lastPosition;
        _movementDelta = Vector3.Lerp(_movementDelta, movement, 25 * Time.deltaTime);

        // Map movement to rotation and smooth it
        Vector3 movementRotation = new Vector3(-_movementDelta.y, _movementDelta.x, 0f) * rotationAmount;
        _rotationDelta = Vector3.Lerp(_rotationDelta, movementRotation, _rotationSpeed * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(
            _rotationDelta.x,
            _rotationDelta.y,
            Mathf.Clamp(_rotationDelta.x, -60, 60)
        );

        transform.position = Vector3.Lerp(_slotTransform.position, _slotTransform.position, followSpeed * Time.deltaTime);
        _lastPosition = _slotTransform.position;
    }

    private void BeginDrag(CardDragHandler card)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        _canvas.overrideSorting = true;
    }

    private void EndDrag(CardDragHandler card)
    {
        _canvas.overrideSorting = false;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(CardDragHandler card)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(CardDragHandler card)
    {
        if (!_dragHandler.wasDragged)
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void OnSelect(CardDragHandler card, bool isSelected)
    {
        if (card.Stats.cardInfo.goID != _dragHandler.Stats.cardInfo.goID) return;

        _isSelected = isSelected;
        var direction = isSelected ? Vector3.up : -Vector3.up;
        tiltParent.localPosition += selectionOffset * direction;
        _canvas.overrideSorting = isSelected;

        if(scaleAnimations){
            var scale = isSelected ? scaleOnSelect : 1f; 
            transform.DOScale(scale, scaleTransition).SetEase(scaleEase);
        }

        if(shakeAnimations){
            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle/2), hoverTransition, 20, 1).SetId(2);
        }
    }

    private float NormalizedSlotPosition() => (float) _dragHandler.ParentIndex() / _dragHandler.SiblingAmount();

    internal void Reset()
    {
        tiltParent.localPosition = Vector3.zero;
        tiltParent.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        _canvas.overrideSorting = false;
    }

    private void OnDestroy() {
        CardSelectionHandler.OnCardSelection += OnSelect;
    }

}
