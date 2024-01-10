using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	[SerializeField] private CardLocation pileType;
	public bool updatePosition;
	public float height = 0.5f;
	public float width = 1f;
	[Range(0f, 90f)] public float maxCardAngle = 5f;
	public float yPerCard = 0f;
	public float zDistance;
	[SerializeField] private Transform _cardHolder;
	[SerializeField] private CardPileUI _cardPileUI;

	private readonly List<GameObject> cards = new List<GameObject>();
	readonly List<GameObject> forceSetPosition = new List<GameObject>();

	public void Add(GameObject card, bool moveAnimation = false) => Add(card, -1, moveAnimation);

	public void Add(GameObject card, int index, bool moveAnimation = true)
	{
		if (index == -1) cards.Add(card);
		else cards.Insert(index, card);

		if (!moveAnimation) forceSetPosition.Add(card);
	}

	public void CardHasArrived(GameObject card)
	{
		card.transform.SetParent(_cardHolder, false);
		updatePosition = true;
		_cardPileUI.UpdateCardPileNumber(cards.Count);
	}

	public void Remove(GameObject card)
	{
		if (!cards.Contains(card)) return;

		cards.Remove(card);
		card.transform.DOKill();

		updatePosition = true;
		_cardPileUI.UpdateCardPileNumber(cards.Count);
	}

	// public void RemoveAt(int index)
	// {
	// 	Remove(cards[index]);
	// }

	public void RemoveAll()
	{
		while (cards.Count > 0)
			Remove(cards[0]);

		updatePosition = true;
	}

	private void UpdateCardPositions()
	{
		float radius = Mathf.Abs(height) < 0.001f
			? width * width / 0.001f * Mathf.Sign(height) 
			: height / 2f + width * width / (8f * height);

		float angle = 2f * Mathf.Asin(0.5f * width / radius) * Mathf.Rad2Deg;
		angle = Mathf.Sign(angle) * Mathf.Min(Mathf.Abs(angle), maxCardAngle * (cards.Count - 1));
		float cardAngle = cards.Count == 1 ? 0f : angle / (cards.Count - 1f);

		for (int i = 0; i < cards.Count; i++)
		{
			cards[i].transform.SetParent(_cardHolder, false);

			Vector3 position = new Vector3(0f, radius, 0f);
			position = Quaternion.Euler(0f, 0f, angle / 2f - cardAngle * i) * position;
			position.y += height - radius;
			position += i * new Vector3(0f, yPerCard, zDistance);

			var rotation = new Vector3(0f, 0f, angle / 2f - cardAngle * i);

			if (!forceSetPosition.Contains(cards[i])) {
				cards[i].transform.DOKill();
				cards[i].transform.DOLocalMove(position, SorsTimings.cardPileRearrangement);
				cards[i].transform.DOLocalRotate(rotation, SorsTimings.cardPileRearrangement);
				cards[i].transform.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
			} else {
				forceSetPosition.Remove(cards[i]);
				cards[i].transform.localPosition = position;
				cards[i].transform.localRotation = Quaternion.Euler(rotation);
				cards[i].transform.localScale = Vector3.one;
			}
		}
	}

	private void LateUpdate(){
		if (!updatePosition) return;
		
		UpdateCardPositions();
		updatePosition = false;
	}
}
