using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

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

        _cardSpawner.SpawnDetailCardObjectsInGrid(cards);
        _cardCollectionUI.Open(listInfo);
    }
}
