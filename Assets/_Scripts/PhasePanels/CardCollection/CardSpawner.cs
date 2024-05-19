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

    public void SpawnDetailCardObjects(List<CardInfo> cards)
    {
        _grid.SetPanelWidth(cards.Count);

        var transforms = new List<Transform>();
        foreach (var cardInfo in cards){
            var detailCardObject = cardInfo.type switch{
                CardType.Creature => Instantiate(_creatureDetailCardPrefab) as GameObject,
                CardType.Technology => Instantiate(_technologyDetailCardPrefab) as GameObject,
                CardType.Money => Instantiate(_moneyDetailCardPrefab) as GameObject,
                _ => throw new System.Exception("Card type not found")
            };

            // Initialize detail card
            var detailCard = detailCardObject.GetComponent<DetailCard>();
            detailCard.SetCardUI(cardInfo);

            transforms.Add(detailCard.gameObject.transform);
        }

        _grid.AddCards(transforms);
    }
}
