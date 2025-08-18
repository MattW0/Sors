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
		var dragHandler = Instantiate(_cardSlotPrefab, parent, false).GetComponentInChildren<CardDragHandler>();
        var cardVisual = Instantiate(_cardVisualPrefab, transform, false).GetComponent<CardVisualHandler>();

        dragHandler.SetCardVisual(cardVisual);
        cardVisual.Initialize(dragHandler, card);

        _playerHand.AddSlot(dragHandler);
    }

    private void CreateDragableSlot(Transform parent)
    {
    }
}
