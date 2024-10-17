using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	[SerializeField] public Transform cardHolderTransform;
	public CardLocation pileType;
	private bool updatePosition;
	[ShowInInspector] public bool UpdatePosition {
		get => updatePosition;
		set {
			updatePosition = value;
			#if UNITY_EDITOR
			UpdateCardsInEditor();
			#endif
		}
	}

	[Header("Arrangement Settings")]
	[SerializeField] private static Vector2 _handWidthDefault = new Vector2(400f, 1000f);
	public CardPileSettings settings;
	private CardPileSettings handSettings = new CardPileSettings(15f, _handWidthDefault.x, 5f, 2f, -3f);
	private CardPileSettings selectionSettings = new CardPileSettings(0f, 100f, 0.1f, 0.1f, -1f);
	private CardPileSettings pileSettings = new CardPileSettings(20f, 20f, 0f, 1f, -1f);

	[SerializeField] private readonly List<GameObject> cards = new List<GameObject>();
	readonly List<GameObject> forceSetPosition = new List<GameObject>();
	private CardPileUI _cardPileUI;

	private void Start()
	{
		_cardPileUI = gameObject.GetComponent<CardPileUI>();
		SetDefaultPileSettings();
	}

    public void CardHasArrived(GameObject card)
	{
		card.transform.SetParent(cardHolderTransform, false);
		updatePosition = true;
		_cardPileUI.UpdateCardPileNumber(cards.Count);
	}

	public void Add(GameObject card, bool moveAnimation = false) => Add(card, -1, moveAnimation);
	public void Add(GameObject card, int index, bool moveAnimation = true)
	{
		if (index == -1) cards.Add(card);
		else cards.Insert(index, card);

		if (!moveAnimation) forceSetPosition.Add(card);
	}

	public void Remove(GameObject card)
	{
		if (!cards.Contains(card)) return;

		cards.Remove(card);
		card.transform.DOKill();

		UpdatePosition = true;
		_cardPileUI.UpdateCardPileNumber(cards.Count);
	}
	public void RemoveAll()
	{
		while (cards.Count > 0)
			Remove(cards[0]);

		UpdatePosition = true;
	}

	private void UpdateCardPositions()
	{
		if (pileType == CardLocation.Hand || pileType == CardLocation.Interaction) 
			ChangePileWidth(cards.Count);

		(float radius, float angle, float cardAngle) = GetGeometry();

		for (int i = 0; i < cards.Count; i++)
		{
			cards[i].transform.SetParent(cardHolderTransform, false);

			(Vector3 position, Vector3 rotation) = GetCardPosition(radius, angle, cardAngle, i); 

			if (forceSetPosition.Contains(cards[i])) {
				forceSetPosition.Remove(cards[i]);
				cards[i].transform.localPosition = position;
				cards[i].transform.localRotation = Quaternion.Euler(rotation);
				cards[i].transform.localScale = Vector3.one;
			} else {
				cards[i].transform.DOKill();
				cards[i].transform.DOLocalMove(position, SorsTimings.cardPileRearrangement);
				cards[i].transform.DOLocalRotate(rotation, SorsTimings.cardPileRearrangement);
				cards[i].transform.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
			}
		}
	}

	private void LateUpdate()
	{
		if (!updatePosition) return;
		updatePosition = false;

		UpdateCardPositions();
	}

	private void UpdateCardsInEditor()
	{
		if (!updatePosition || Application.isPlaying) return;
		updatePosition = false;

		cards.Clear();
		forceSetPosition.Clear();
		foreach (Transform child in cardHolderTransform) Add(child.gameObject);

		UpdateCardPositions();
	}

	private (float, float, float) GetGeometry()
	{
		float radius = Mathf.Abs(settings.height) < 0.001f
			? settings.width * settings.width / 0.001f * Mathf.Sign(settings.height) 
			: settings.height / 2f + settings.width * settings.width / (8f * settings.height);

		float angle = 2f * Mathf.Asin(0.5f * settings.width / radius) * Mathf.Rad2Deg;
		angle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), settings.maxCardAngle * (cards.Count - 1));
		float cardAngle = cards.Count == 1 ? 0f : angle / (cards.Count - 1f);

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

    internal void StartInteraction()
	{
		settings = handSettings;
		updatePosition = true;
	}

    internal void EndInteraction() => SetDefaultPileSettings();
	private void SetDefaultPileSettings()
    {
        settings = pileType switch
		{
			CardLocation.Hand => handSettings,
			CardLocation.Selection => selectionSettings,
			_ => pileSettings
		};
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