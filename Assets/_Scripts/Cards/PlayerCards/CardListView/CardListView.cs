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
        print("CardListView open collection with " + cards.Count + " cards - Location: " + listInfo.location);

        _cardSpawner.SpawnDetailCardObjectsInGrid(cards);
        _cardCollectionUI.Open(listInfo);
    }
}
