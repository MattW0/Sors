using UnityEngine;

[System.Serializable]
public class CardSlot : MonoBehaviour
{
    public CardDragHandler DragHandler { get; private set; }
    public CardVisualHandler VisualHandler { get; private set; }

    private void Awake()
    {
        DragHandler = GetComponentInChildren<CardDragHandler>();
        VisualHandler = DragHandler.GetComponentInChildren<CardVisualHandler>();
    }

    public void MoveToPile(Transform pileTransform)    
    {
        print("Set parent: " + pileTransform);
        transform.SetParent(pileTransform, false);
    }
    
    public void Initialize(GameObject card)
    {
        DragHandler.Initialize(card.GetComponent<CardClickHandler>(), VisualHandler);
        gameObject.SetActive(true);
    }

    public void DetachToPool(Transform poolParent)
    {
        gameObject.SetActive(false);
        transform.SetParent(poolParent, false);
    }
}
