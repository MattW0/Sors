using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CardGrid), typeof(CardSpawner), typeof(CardCollectionUI))]
public class CardCollectionManager : MonoBehaviour
{
    private CardSpawner _cardSpawner;
    private CardCollectionUI _cardCollectionUI;

    private void Awake()
    {
        _cardSpawner = GetComponent<CardSpawner>();
        _cardCollectionUI = GetComponent<CardCollectionUI>();
    }

    public void OpenCardCollection(List<CardStats> collection, CardLocation collectionType, bool ownsCollection)
    {
        print("CardInfos count: " + collection.Count);

        _cardCollectionUI.Open(collectionType, ownsCollection);
        _cardSpawner.SpawnDetailCardObjectsInGrid(collection);
    }

    public void UpdateCollection(List<CardStats> cards)
    {
        print("Updating card collection");
        _cardSpawner.SpawnDetailCardObjectsInGrid(cards);
    }
}
