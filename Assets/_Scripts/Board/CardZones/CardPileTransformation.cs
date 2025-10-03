using UnityEngine;
using System.Linq;
using UnityUtils;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(CardPileUI))]
public class CardPileTransformation : Transformable
{
	[HideInInspector] public Transform cardHolderTransform;
	private CardPileUI _cardPileUI;
    [SerializeField] private CardPileSettings _defaultSettings;
    [SerializeField] private CardPileSettings _interactionSettings;
    public CardPileCurveParameters curve;
    private CardPileSettings _active;
    private ICardPile _pile;
    private float _width;
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float _minWidth = 200f;
    [SerializeField] private float _maxWidth = 800f;
    [SerializeField] private HorizontalLayoutGroup _layoutGroup;
    private CancellationToken _cancellationToken;
    
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

        _cancellationToken = this.GetCancellationTokenOnDestroy();
        InitTransformable(cardHolderTransform);
        StartMove(false).Forget();
	}

    internal async UniTaskVoid StartTransform() => await DoTransform();
    internal async UniTaskVoid StartMove(bool isInteraction)
	{
        _active = isInteraction ? _interactionSettings : _defaultSettings;

        await UniTask.WhenAll(
            MoveTask(_cancellationToken, _active, SorsTimings.cardPileRearrangement),
            DoTransform()
        );

        _layoutGroup.enabled = _active.isHorizontalLayout;
	}


    private async UniTask DoTransform()
    {
        if(_active.isHorizontalLayout) {
            _width = Math.Min(_pile.NumberCards * cardWidth, _maxWidth);
            _width = Math.Max(_width, _minWidth);
        } else {
            _width = 0f;
        }

        await TransformTask(_cancellationToken, _width, SorsTimings.cardPileRearrangement);
    }
}