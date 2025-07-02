using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro.EditorUtilities;


public class CardVisualHandler : MonoBehaviour
{
    private bool initalize = false;

    [Header("CardDragHandler")]
    public CardDragHandler parentCard;
    private Transform cardTransform;
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

    public void Initialize(CardDragHandler parent, int index = 0)
    {
        //Declarations
        parentCard = parent;
        cardTransform = parent.transform;
        _lastPosition = cardTransform.position;

        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;
    }

    void LateUpdate()
    {
        if (!initalize || parentCard == null) return;

        // Apply curve y position
        HandPositioning();

        // Apply static rotation or hover tilt
        CardTilt();

        if (! parentCard.isDragging ) return;
        FollowDrag();
    }

    private void HandPositioning()
    {
        var normalPosition = NormalizedSlotPosition();
        curveYOffset = curve.positioning.Evaluate(normalPosition) * curve.positioningInfluence;
        
        curveYOffset = SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(normalPosition);

        if (parentCard.isDragging) return;
        transform.position = new Vector3(transform.position.x, curveYOffset, transform.position.z);
    }

    private void CardTilt()
    {
        if (parentCard.isDragging) return;

        savedIndex = parentCard.isDragging ? savedIndex : parentCard.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

        Vector3 offset = transform.position - MouseInputHelper.GetMouseWorldPosition(_cam);
        float tiltX = parentCard.isHovering ? (offset.y * -1 * manualTiltAmount) : 0;
        float tiltY = parentCard.isHovering ? (offset.x * manualTiltAmount) : 0;
        float tiltZ = curveRotationOffset * (curve.rotationInfluence * SiblingAmount());

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    private void FollowDrag()
    {
        Vector3 movement = cardTransform.position - _lastPosition;
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);

        // Map movement to rotation and smooth it
        Vector3 movementRotation = new Vector3(-movementDelta.y, movementDelta.x, 0f) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, _rotationSpeed * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(
            rotationDelta.x,
            rotationDelta.y,
            Mathf.Clamp(rotationDelta.x, -60, 60)
        );

        transform.position = Vector3.Lerp(transform.position, cardTransform.position, followSpeed * Time.deltaTime);
        _lastPosition = cardTransform.position;
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
        if (!parentCard.wasDragged)
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

    private int SiblingAmount() => transform.parent.parent.parent.childCount - 1;
    private float NormalizedSlotPosition() => (float) parentCard.ParentIndex() / SiblingAmount();
}
