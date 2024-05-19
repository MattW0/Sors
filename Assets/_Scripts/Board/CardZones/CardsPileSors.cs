using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	[SerializeField] private Transform _cardHolder;
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
	public float height = 0.5f;
	public float width = 1f;
	private float _defaultHandWidth = 400f;
	[Range(0f, 90f)] public float maxCardAngle = 5f;
	public float yPerCard = 0f;
	public float zDistance;
	private readonly List<GameObject> cards = new List<GameObject>();
	readonly List<GameObject> forceSetPosition = new List<GameObject>();
	private CardPileUI _cardPileUI;

	private void Start(){
		_cardPileUI = gameObject.GetComponent<CardPileUI>();
	}

	public void CardHasArrived(GameObject card)
	{
		card.transform.SetParent(_cardHolder, false);
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
		if (pileType == CardLocation.Hand) ChangeHandDimensions(cards.Count);

		(float radius, float angle, float cardAngle) = GetGeometry();

		for (int i = 0; i < cards.Count; i++)
		{
			cards[i].transform.SetParent(_cardHolder, false);

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
		foreach (Transform child in _cardHolder) Add(child.gameObject);

		UpdateCardPositions();
	}

	private (float, float, float) GetGeometry()
	{
		float radius = Mathf.Abs(height) < 0.001f
			? width * width / 0.001f * Mathf.Sign(height) 
			: height / 2f + width * width / (8f * height);

		float angle = 2f * Mathf.Asin(0.5f * width / radius) * Mathf.Rad2Deg;
		angle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), maxCardAngle * (cards.Count - 1));
		float cardAngle = cards.Count == 1 ? 0f : angle / (cards.Count - 1f);

		return (radius, angle, cardAngle);
	}

	private (Vector3, Vector3) GetCardPosition(float radius, float angle, float cardAngle, int i)
	{
		Vector3 position = new Vector3(0f, radius, 0f);
		position = Quaternion.Euler(0f, 0f, angle / 2f - cardAngle * i) * position;
		position.y += height - radius;
		position += i * new Vector3(0f, yPerCard, zDistance);

		var rotation = new Vector3(0f, 0f, angle / 2f - cardAngle * i);

		return (position, rotation);
	}

	private void ChangeHandDimensions(int cardCount)
	{
		// Changes the width of the hand based on the number of cards
		// TODO: Use groupings for the same card(s) like in dominion.games ?
		if (cardCount < 6) width = _defaultHandWidth;
		else if (cardCount < 10) width = _defaultHandWidth * 1.5f;
		else if (cardCount < 14) width = _defaultHandWidth * 2f;
		else width = _defaultHandWidth * 2.5f;
	}
}
