using UnityEngine;

[System.Serializable]
public class CardSlot : MonoBehaviour
{
    public CardDragHandler DragHandler { get; private set; }
    private CardVisualHandler _visualHandler;

    public void Initialize(CardStats stats, CardPileCurveParameters curve)
    {
        DragHandler = GetComponentInChildren<CardDragHandler>();
        _visualHandler = DragHandler.GetComponentInChildren<CardVisualHandler>();

        stats.DragHandler = DragHandler;
        DragHandler.Initialize(_visualHandler, stats, curve);
        gameObject.SetActive(true);
    }

    public void SetParent(Transform pileTransform)
    {
        transform.SetParent(pileTransform, false);
        transform.localPosition = Vector3.zero;
    } 

    public void DetachToPool(Transform poolParent)
    {
        gameObject.SetActive(false);
        transform.SetParent(poolParent, false);
    }
}
