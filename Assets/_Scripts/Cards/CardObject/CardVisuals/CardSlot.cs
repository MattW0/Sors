using UnityEngine;

[System.Serializable]
public class CardSlot : MonoBehaviour
{
    public CardDragHandler DragHandler { get; private set; }
    public CardVisualHandler VisualHandler { get; private set; }

    private void Awake() {
        DragHandler = GetComponentInChildren<CardDragHandler>();
        VisualHandler = DragHandler.GetComponentInChildren<CardVisualHandler>();
    }

    public void SetCard(GameObject card) {
        DragHandler.Initialize(card.GetComponent<CardClickHandler>(), VisualHandler);
    }
}
