using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardsPileSors : MonoBehaviour
{
	[SerializeField] private CardLocation pileType;
	public bool updatePosition;
	public float height = 0.5f;
	public float width = 1f;
	[Range(0f, 90f)] public float maxCardAngle = 5f;
	public float yPerCard = 0f;
	public float zDistance;

	public float moveDuration = 0.5f;
	public Transform cardHolder;

	readonly List<GameObject> cards = new List<GameObject>();
	public List<GameObject> Cards => new List<GameObject>(cards);

	public event Action<int> OnCountChanged;
	readonly List<GameObject> forceSetPosition = new List<GameObject>();

	private void Awake(){
		if (pileType == CardLocation.Hand){
			height = 14f;
			width = 500f;
			maxCardAngle = 90f;
			zDistance = -0.1f;
		}
	}

	public void Add(GameObject card, bool moveAnimation = true) => Add(card, -1, moveAnimation);

	public void Add(GameObject card, int index, bool moveAnimation = true)
	{
		// Transform cardHolder = GetCardHolder();
		card.transform.SetParent(cardHolder, false);

		if (index == -1) cards.Add(card);
		else cards.Insert(index, card);

		if (!moveAnimation) forceSetPosition.Add(card);

		OnCountChanged?.Invoke(cards.Count);

		UpdateCardPositions();
	}

	public void Remove(GameObject card)
	{
		if (!cards.Contains(card)) return;

		cards.Remove(card);
		card.transform.DOKill();

		OnCountChanged?.Invoke(cards.Count);

		UpdateCardPositions();
	}

	public void RemoveAt(int index)
	{
		Remove(cards[index]);
		UpdateCardPositions();
	}

	public void RemoveAll()
	{
		while (cards.Count > 0)
			Remove(cards[0]);

		UpdateCardPositions();
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
			cards[i].transform.SetParent(cardHolder, false);

			Vector3 position = new Vector3(0f, radius, 0f);
			position = Quaternion.Euler(0f, 0f, angle / 2f - cardAngle * i) * position;
			position.y += height - radius;
			position += i * new Vector3(0f, yPerCard, zDistance);

			var rotation = new Vector3(0f, 0f, angle / 2f - cardAngle * i);

			if (!forceSetPosition.Contains(cards[i])) {
				cards[i].transform.DOKill();
				cards[i].transform.DOLocalMove(position, moveDuration);
				cards[i].transform.DOLocalRotate(rotation, moveDuration);
				cards[i].transform.DOScale(Vector3.one, moveDuration);
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
