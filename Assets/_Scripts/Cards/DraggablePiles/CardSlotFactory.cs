using UnityEngine;

public class CardSlotFactory : ICardSlotFactory
{
    private readonly GameObject _slotPrefab;
    private readonly Transform _poolParent;

    public CardSlotFactory(GameObject slotPrefab, Transform poolParent)
    {
        _slotPrefab = slotPrefab;
        _poolParent = poolParent;
    }

    public CardSlot CreateSlot()
    {
        var slotGO = Object.Instantiate(_slotPrefab, _poolParent, false);
        var slot = slotGO.GetComponent<CardSlot>();
        slotGO.SetActive(false);
        return slot;
    }

    public void DestroySlot(CardSlot slot)
    {
        MonoBehaviour.Destroy(slot.gameObject);
    }
}

public interface ICardSlotFactory
{
    CardSlot CreateSlot();
    void DestroySlot(CardSlot slot);
}