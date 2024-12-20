using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CardSpawner), typeof(CardListUI))]
public class CardListView : MonoBehaviour
{
    private CardSpawner _cardSpawner;
    private CardListUI _cardCollectionUI;

    private void Awake()
    {
        _cardSpawner = GetComponent<CardSpawner>();
        _cardCollectionUI = GetComponent<CardListUI>();
    }

    public void OpenCardCollection(List<CardStats> cards, CardListInfo listInfo)
    {
        print("CardInfos count: " + cards.Count);

        _cardCollectionUI.Open(listInfo);
        _cardSpawner.SpawnDetailCardObjectsInGrid(cards);
    }

    public void UpdateCollection(List<CardStats> cards)
    {
        print("Updating card collection");
        _cardSpawner.SpawnDetailCardObjectsInGrid(cards);
    }
}
