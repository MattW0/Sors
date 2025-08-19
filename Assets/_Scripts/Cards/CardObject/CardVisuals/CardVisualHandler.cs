using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro.EditorUtilities;


public class CardVisualHandler : MonoBehaviour
{
    private bool initalize = false;

    [Header("CardDragHandler")]
    public CardDragHandler _dragHandler;
    private Transform _slotTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;
    [SerializeField] private float _rotationSpeed = 10;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float autoTiltAmount = 5;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hober Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 3;

    [Header("Curve")]
    [SerializeField] private CardPileCurveParameters curve;

    private Vector3 _lastPosition;
    private float curveYOffset;
    private float curveRotationOffset;
    private Camera _cam; 

    private void Start()
    {
        _cam = Camera.main;
        shadowDistance = visualShadow.localPosition;
    }

    public void Initialize(CardDragHandler dragHandler, GameObject card, int index = 0)
    {
        //Declarations
        _dragHandler = dragHandler;
        _slotTransform = dragHandler.transform;
        _lastPosition = transform.position;

        card.transform.SetParent(tiltParent, false);
        card.transform.localPosition = Vector3.zero;

        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        _dragHandler.PointerEnterEvent.AddListener(PointerEnter);
        _dragHandler.PointerExitEvent.AddListener(PointerExit);
        _dragHandler.BeginDragEvent.AddListener(BeginDrag);
        _dragHandler.EndDragEvent.AddListener(EndDrag);
        _dragHandler.PointerDownEvent.AddListener(PointerDown);
        _dragHandler.PointerUpEvent.AddListener(PointerUp);
        _dragHandler.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;
    }

    void LateUpdate()
    {
        if (!initalize || _dragHandler == null) return;

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

        curveRotationOffset = curve.rotation.Evaluate(normalPosition);
        curveYOffset = curve.positioning.Evaluate(normalPosition) * curve.positioningInfluence;
        if (_dragHandler.SiblingAmount() < 5) curveYOffset = 0;

        if (_dragHandler.isDragging) return;
        transform.localPosition = new Vector3(_slotTransform.position.x, curveYOffset, _slotTransform.position.z);
    }

    private void CardTilt()
    {
        if (_dragHandler.isDragging) return;

        savedIndex = _dragHandler.isDragging ? savedIndex : _dragHandler.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (_dragHandler.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (_dragHandler.isHovering ? .2f : 1);

        Vector3 offset = transform.position - MouseInputHelper.GetMouseWorldPosition(_cam);
        float tiltX = _dragHandler.isHovering ? (offset.y * -1 * manualTiltAmount) : 0;
        float tiltY = _dragHandler.isHovering ? (offset.x * manualTiltAmount) : 0;
        float tiltZ = curveRotationOffset * (curve.rotationInfluence * _dragHandler.SiblingAmount());

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
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);

        // Map movement to rotation and smooth it
        Vector3 movementRotation = new Vector3(-movementDelta.y, movementDelta.x, 0f) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, _rotationSpeed * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(
            rotationDelta.x,
            rotationDelta.y,
            Mathf.Clamp(rotationDelta.x, -60, 60)
        );

        transform.position = Vector3.Lerp(_slotTransform.position, _slotTransform.position, followSpeed * Time.deltaTime);
        _lastPosition = _slotTransform.position;
    }

    private void Select(CardDragHandler card, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle/2), hoverTransition, 20, 1).SetId(2);

        if(scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
    }

    public void Swap(float swapDirection = 1)
    {
        // Swap direction = -1 (right), 1 (left)
        if (!swapAnimations)
            return;

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * swapRotationAngle * swapDirection, swapTransition, swapVibrato, 1).SetId(3);
    }

    private void BeginDrag(CardDragHandler card)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    private void EndDrag(CardDragHandler card)
    {
        canvas.overrideSorting = false;
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

    private void PointerUp(CardDragHandler card, bool longPress)
    {
        if(scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
        canvas.overrideSorting = false;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    private void PointerDown(CardDragHandler card)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
            
        visualShadow.localPosition += -Vector3.up * shadowOffset;
        shadowCanvas.overrideSorting = false;
    }

    private float NormalizedSlotPosition() => (float) _dragHandler.ParentIndex() / _dragHandler.SiblingAmount();
}
