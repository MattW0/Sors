using UnityEngine;
using UnityUtils;

public class CardDragManager : MonoBehaviour
{
    [SerializeField] private SortableCardPile _playerHand;
    [SerializeField] private GameObject _cardSlotPrefab;
    [SerializeField] private GameObject _cardVisualPrefab;

    public void MakeCardDraggable(GameObject card, Transform parent)
    {
        // CreateDragableSlot(parent);
		var cardSlot = Instantiate(_cardSlotPrefab, parent, false);
        var cardVisual = Instantiate(_cardVisualPrefab, cardSlot.transform, false).GetComponent<CardVisualHandler>();

        var dragHandler = cardSlot.GetComponentInChildren<CardDragHandler>();
        dragHandler.SetCardVisual(cardVisual);

        cardVisual.Initialize(dragHandler, card);
        _playerHand.AddSlot(dragHandler);
    }

    private void CreateDragableSlot(Transform parent)
    {
    }
}
