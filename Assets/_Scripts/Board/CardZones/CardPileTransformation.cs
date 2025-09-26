using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityUtils;
using UnityEngine.Scripting.APIUpdating;
using TMPro.EditorUtilities;

[RequireComponent(typeof(CardPileUI))]
public class CardPileTransformation : Transformable
{
	[HideInInspector] public Transform cardHolderTransform;
	private CardPileUI _cardPileUI;
    [SerializeField] private CardPileSettings _defaultSettings;
    [SerializeField] private CardPileSettings _interactionSettings;
    private CardPileSettings _active;
    [SerializeField] bool _inEditor = false;
    
	private void Awake() 
	{
		_cardPileUI = GetComponent<CardPileUI>();
        Default = _defaultSettings;
        Transformed = _interactionSettings;
    }

    private void Start()
	{
		cardHolderTransform = transform.Children().First().transform;
		_cardPileUI.ParentTransform = cardHolderTransform;

        InitTransformable(cardHolderTransform);
        EndInteraction();
	}

    private void Update() 
    {
        if (_inEditor || _active == null) return;

        StartMove(_active.position, _active.scale);
        StartTransform(_active, SorsTimings.cardPileRearrangement);
    }

    internal void StartInteraction()
	{
        _active = _interactionSettings;
        StartMove(_active.position, _active.scale);
	}

    internal void EndInteraction()
	{
        _active = _defaultSettings ?? null;
        
        if(_active) StartMove(_active.position, _active.scale);
        else StartMove(Vector3.zero);
	}
}