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
    [SerializeField] private CardDragHandler _movingCard;
    bool isCrossing = false;


	// [ShowInInspector] public bool UpdatePosition {
	// 	get => _updatePosition;
	// 	set {
	// 		_updatePosition = value;
	// 		#if UNITY_EDITOR
	// 		UpdateCardsInEditor();
	// 		#endif
	// 	}
	// }

	[Header("Arrangement Settings")]
	[SerializeField] private static Vector2 _handWidthDefault = new Vector2(400f, 1000f);
	public CardPileSettings settings;
	private CardPileSettings handSettings = new(15f, _handWidthDefault.x, 5f, 2f, -3f);
	private CardPileSettings selectionSettings = new(0f, 100f, 0.1f, 0.1f, -1f);
	private CardPileSettings pileSettings = new(20f, 20f, 0f, 1f, -1f);
	private CardPileUI _cardPileUI;
    public List<CardDragHandler> _dragHandlers;


	private void Start()
	{
		_cardPileUI = gameObject.GetComponent<CardPileUI>();
		SetDefaultPileSettings();
	}

	private void LateUpdate()
	{
		if (_movingCard == null || isCrossing) return;
        CheckSlotPosition();

		// UpdateCardPositions();
	}

    public void CardHasArrived(GameObject card)
	{
		card.transform.SetParent(cardHolderTransform, false);
		_cardPileUI.UpdateCardPileNumber(cardHolderTransform.childCount);

		_dragHandlers.Add(card.GetComponent<CardDragHandler>());
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
	
	private void SetDefaultPileSettings()
    {
		if (pileType == CardLocation.Selection) settings = selectionSettings;
		else if (pileType == CardLocation.Discard 
				|| pileType == CardLocation.Deck
				|| pileType == CardLocation.Trash) 
					settings = pileSettings;
    }

	private void CheckSlotPosition()
    {
        for (int i = 0; i < _dragHandlers.Count; i++)
        {
            if (_movingCard.transform.position.x > _dragHandlers[i].transform.position.x)
            {
                if (_movingCard.ParentIndex() < _dragHandlers[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (_movingCard.transform.position.x < _dragHandlers[i].transform.position.x)
            {
                if (_movingCard.ParentIndex() > _dragHandlers[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    private void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = _movingCard.transform.parent;
        Transform crossedParent = _dragHandlers[index].transform.parent;

        _dragHandlers[index].transform.SetParent(focusedParent);
        _dragHandlers[index].transform.localPosition =
			 _dragHandlers[index].selected ? new Vector3(0, _dragHandlers[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        isCrossing = false;

        int swapDirection = _dragHandlers[index].ParentIndex() > _movingCard.ParentIndex() ? -1 : 1;
        _dragHandlers[index].Swap(swapDirection);
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