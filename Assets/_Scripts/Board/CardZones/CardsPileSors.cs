using System;
using UnityEngine;

[RequireComponent(typeof(CardPileUI))]
public class CardsPileSors : MonoBehaviour
{
	public Transform cardHolderTransform;
	public CardLocation pileType;

	[Header("Arrangement Settings")]
	// [SerializeField] private static Vector2 _handWidthDefault = new Vector2(400f, 1000f);
	// public CardPileSettings settings;
	// private CardPileSettings handSettings = new(15f, _handWidthDefault.x, 5f, 2f, -3f);
	// private CardPileSettings selectionSettings = new(0f, 100f, 0.1f, 0.1f, -1f);
	// private CardPileSettings pileSettings = new(20f, 20f, 0f, 1f, -1f);
	private CardPileUI _cardPileUI;


	private void Awake() {
		CardMover.OnUpdatePileNumbers += CheckNumberCards;

	}

	private void Start()
	{
		_cardPileUI = GetComponent<CardPileUI>();
		// cardHolderTransform.position = _cardPileUI.transform.position;
	}

	private void CheckNumberCards()
	{
		_cardPileUI.UpdateCardPileNumber(cardHolderTransform.childCount);
	}

	internal void StartInteraction()
	{
		// settings = handSettings;
	}

    internal void EndInteraction()
	{
		// SetDefaultPileSettings();
	}
	
	// private void SetDefaultPileSettings()
    // {
	// 	if (pileType == CardLocation.Selection) settings = selectionSettings;
	// 	else if (pileType == CardLocation.Discard 
	// 			|| pileType == CardLocation.Deck
	// 			|| pileType == CardLocation.Trash) 
	// 				settings = pileSettings;
    // }

	private void OnDestroy() {
		CardMover.OnUpdatePileNumbers -= CheckNumberCards;
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