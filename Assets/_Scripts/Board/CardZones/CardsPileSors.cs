using System;
using UnityEngine;
using DG.Tweening;
using UnityUtils;
using System.Linq;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	public CardLocation pileType;

	[Header("Arrangement Settings")]
	[SerializeField] public CardPileSettings defaultSettings;
	[SerializeField] public CardPileSettings interactionSettings;
	[HideInInspector] public Transform cardHolderTransform;
	private CardPileUI _cardPileUI;

    private void Start()
	{
		cardHolderTransform = transform.Children().First().transform;
		
		_cardPileUI = GetComponent<CardPileUI>();
		_cardPileUI.ParentTransform = cardHolderTransform;
	}

	internal void StartInteraction()
	{
		cardHolderTransform.DOLocalMove(interactionSettings.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(interactionSettings.scale, SorsTimings.cardPileRearrangement);
	}

    internal void EndInteraction()
	{
		cardHolderTransform.DOLocalMove(defaultSettings.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(defaultSettings.scale, SorsTimings.cardPileRearrangement);
	}
}