using UnityEngine;

[System.Serializable]
public class CardSlot : MonoBehaviour
{
    public CardDragHandler DragHandler { get; private set; }
    private CardVisualHandler _visualHandler;

    public void Initialize(GameObject card)
    {
        DragHandler = GetComponentInChildren<CardDragHandler>();
        _visualHandler = DragHandler.GetComponentInChildren<CardVisualHandler>();

        DragHandler.Initialize(card.GetComponent<CardClickHandler>(), _visualHandler);
        gameObject.SetActive(true);
    }

    public void SetParent(Transform pileTransform) => transform.SetParent(pileTransform, false);
    

    public void DetachToPool(Transform poolParent)
    {
        gameObject.SetActive(false);
        transform.SetParent(poolParent, false);
    }
}
