using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	public Transform cardHolderTransform;
	public CardLocation pileType;
	private bool _updatePosition;
	[ShowInInspector] public bool UpdatePosition {
		get => _updatePosition;
		set {
			_updatePosition = value;
			#if UNITY_EDITOR
			UpdateCardsInEditor();
			#endif
		}
	}

	[Header("Arrangement Settings")]
	[SerializeField] private static Vector2 _handWidthDefault = new Vector2(400f, 1000f);
	public CardPileSettings settings;
	private CardPileSettings handSettings = new(15f, _handWidthDefault.x, 5f, 2f, -3f);
	private CardPileSettings selectionSettings = new(0f, 100f, 0.1f, 0.1f, -1f);
	private CardPileSettings pileSettings = new(20f, 20f, 0f, 1f, -1f);
	private CardPileUI _cardPileUI;

	private void Start()
	{
		_cardPileUI = gameObject.GetComponent<CardPileUI>();
		SetDefaultPileSettings();
	}

	private void LateUpdate()
	{
		if (!_updatePosition) return;
		_updatePosition = false;

		UpdateCardPositions();
	}

	private void UpdateCardsInEditor()
	{
		if (!_updatePosition || Application.isPlaying) return;
		_updatePosition = false;

		UpdateCardPositions();
	}

    public void CardHasArrived(GameObject card)
	{
		card.transform.SetParent(cardHolderTransform, false);
		_cardPileUI.UpdateCardPileNumber(cardHolderTransform.childCount);
		_updatePosition = true;
	}

	private void UpdateCardPositions()
	{
		if (pileType == CardLocation.Hand || pileType == CardLocation.Interaction) 
			ChangePileWidth(cardHolderTransform.childCount);

		(float radius, float angle, float cardAngle) = GetGeometry();
		int i = 0;
		foreach (Transform cardTransform in cardHolderTransform)
		{
			(Vector3 position, Vector3 rotation) = GetCardPosition(radius, angle, cardAngle, i); 

			cardTransform.DOKill();
			cardTransform.DOLocalMove(position, SorsTimings.cardPileRearrangement);
			cardTransform.DOLocalRotate(rotation, SorsTimings.cardPileRearrangement);
			cardTransform.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
			i++;
		}
	}

	internal void StartInteraction()
	{
		settings = handSettings;
		_updatePosition = true;
	}

    internal void EndInteraction()
	{
		SetDefaultPileSettings();
		_updatePosition = true;	
	}

	private (float, float, float) GetGeometry()
	{
		float radius = Mathf.Abs(settings.height) < 0.001f
			? settings.width * settings.width / 0.001f * Mathf.Sign(settings.height) 
			: settings.height / 2f + settings.width * settings.width / (8f * settings.height);

		float angle = 2f * Mathf.Asin(0.5f * settings.width / radius) * Mathf.Rad2Deg;
		angle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), settings.maxCardAngle * (cardHolderTransform.childCount - 1));
		float cardAngle = cardHolderTransform.childCount == 1 ? 0f : angle / (cardHolderTransform.childCount - 1f);

		return (radius, angle, cardAngle);
	}

	private (Vector3, Vector3) GetCardPosition(float radius, float angle, float cardAngle, int i)
	{
		Vector3 position = new Vector3(0f, radius, 0f);
		position = Quaternion.Euler(0f, 0f, angle / 2f - cardAngle * i) * position;
		position.y += settings.height - radius;
		position += i * new Vector3(0f, settings.yPerCard, settings.zDistance);

		var rotation = new Vector3(0f, 0f, angle / 2f - cardAngle * i);

		return (position, rotation);
	}

	private void ChangePileWidth(int cardCount)
	{
		// Changes the width of the hand based on the number of cards
		// TODO: Use groupings for the same card(s) like in dominion.games ?
		if (cardCount < 6) settings.width = _handWidthDefault.x;
		else if (cardCount > 16) settings.width = _handWidthDefault.y;
		else settings.width = _handWidthDefault.x + (cardCount - 6) * (_handWidthDefault.y - _handWidthDefault.x) / 10f;
	}
	
	private void SetDefaultPileSettings()
    {
		if (pileType == CardLocation.Selection) settings = selectionSettings;
		else if (pileType == CardLocation.Discard 
				|| pileType == CardLocation.Deck
				|| pileType == CardLocation.Trash) 
					settings = pileSettings;
    }
}

[Serializable]
public struct CardPileSettings
{
	public float height;
	public float width;
	[Range(0f, 90f)] public float maxCardAngle;
	public float yPerCard;
	public float zDistance;

	public CardPileSettings(float height, float width, float maxCardAngle, float yPerCard, float zDistance)
	{
		this.height = height;
		this.width = width;
		this.maxCardAngle = maxCardAngle;
		this.yPerCard = yPerCard;
		this.zDistance = zDistance;
	}
}