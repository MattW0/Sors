using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityUtils;
using UnityEngine.Scripting.APIUpdating;
using TMPro.EditorUtilities;
using System;

[RequireComponent(typeof(CardPileUI))]
public class CardPileTransformation : Transformable
{
	[HideInInspector] public Transform cardHolderTransform;
	private CardPileUI _cardPileUI;
    [SerializeField] private CardPileSettings _defaultSettings;
    [SerializeField] private CardPileSettings _interactionSettings;
    private CardPileSettings _active;
    private ICardPile _pile;
    private float _width;
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float _minWidth = 200f;
    [SerializeField] private float _maxWidth = 800f;

    
	private void Awake() 
	{
        _pile = GetComponent<ICardPile>();
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
        if (_active == null) return;

        StartMove(_active.position, _active.scale);
        if(!_active.isHorizontalLayout) return;

        _width = Math.Min(_pile.NumberCards * cardWidth, _maxWidth);
        _width = Math.Max(_width, _minWidth);

        StartTransform(_active, _width, SorsTimings.cardPileRearrangement);
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