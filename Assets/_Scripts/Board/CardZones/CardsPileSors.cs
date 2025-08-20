using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	public Transform cardHolderTransform;
	public CardLocation pileType;
    public bool isSortable;

	[Header("Arrangement Settings")]
	[SerializeField] public CardPileSettings defaultSettings;
	[SerializeField] public CardPileSettings interactionSettings;
	private CardPileUI _cardPileUI;

    private void Start()
	{
		_cardPileUI = GetComponent<CardPileUI>();
		_cardPileUI.CardHolder = cardHolderTransform;
	}

	internal void StartInteraction()
	{
		print("Start interaction on:" + pileType);
		cardHolderTransform.DOLocalMove(interactionSettings.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(interactionSettings.scale, SorsTimings.cardPileRearrangement);
	}

    internal void EndInteraction()
	{
		cardHolderTransform.DOLocalMove(defaultSettings.position, SorsTimings.cardPileRearrangement);
        cardHolderTransform.DOScale(defaultSettings.scale, SorsTimings.cardPileRearrangement);
	}
}