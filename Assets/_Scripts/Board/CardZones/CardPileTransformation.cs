using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityUtils;

[RequireComponent(typeof(CardPileUI))]
public class CardPileTransformation : Transformable
{
	[HideInInspector] public Transform cardHolderTransform;
	private CardPileUI _cardPileUI;
    [SerializeField] private CardPileSettings _defaultSettings;
    [SerializeField] private CardPileSettings _interactionSettings;
    
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

        InitTransformable();
	}

    private void Update() 
    {
        if (Default == null) return;

        StartTransform(Default, SorsTimings.cardPileRearrangement);
    }

    internal void StartInteraction()
	{
		cardHolderTransform.DOLocalMove(Transformed.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(Transformed.scale, SorsTimings.cardPileRearrangement);
	}

    internal void EndInteraction()
	{
		cardHolderTransform.DOLocalMove(Default.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(Default.scale, SorsTimings.cardPileRearrangement);
	}
}