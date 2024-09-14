using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _creatureDetailCardPrefab;
    [SerializeField] private GameObject _technologyDetailCardPrefab;
    [SerializeField] private GameObject _moneyDetailCardPrefab;
    [SerializeField] private CardGrid _grid;
    [SerializeField] private Transform _spawnParentTransform;

    public void SpawnDetailCardObjectsInGrid(List<CardInfo> cards)
    {
        _grid.SetPanelWidth(cards.Count);

        var transforms = new List<Transform>();
        foreach (var cardInfo in cards){
            transforms.Add(InstantiateCard(cardInfo));
        }

        _grid.AddCards(transforms);
    }

    public List<GameObject> SpawnDetailCardObjects(List<CardInfo> cards)
    {
        var cardObjects = new List<GameObject>();
        foreach (var cardInfo in cards){
            var go = InstantiateCard(cardInfo);
            go.SetParent(_spawnParentTransform, false);
            cardObjects.Add(go.gameObject);
        }

        return cardObjects;
    }

    private Transform InstantiateCard(CardInfo cardInfo){
        var detailCardObject = cardInfo.type switch{
                CardType.Creature => Instantiate(_creatureDetailCardPrefab) as GameObject,
                CardType.Technology => Instantiate(_technologyDetailCardPrefab) as GameObject,
                CardType.Money => Instantiate(_moneyDetailCardPrefab) as GameObject,
                _ => throw new System.Exception("Card type not found")
            };

        // Initialize detail card
        var detailCard = detailCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);

        return detailCardObject.transform;
    }
}
