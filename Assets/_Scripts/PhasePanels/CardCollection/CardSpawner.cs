using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _creatureDetailCardPrefab;
    [SerializeField] private GameObject _technologyDetailCardPrefab;
    [SerializeField] private GameObject _moneyDetailCardPrefab;
    // [SerializeField] private Transform _gridAll;
    // [SerializeField] private Transform _gridChosen;
    [SerializeField] private Transform _grid;


    public List<DetailCard> SpawnDetailCardObjects(List<CardInfo> cards, TurnState turnState, float scaleFactor = 0.6f)
    {
        var detailCards = new List<DetailCard>();
        foreach (var cardInfo in cards){
            var detailCardObject = cardInfo.type switch{
                CardType.Creature => Instantiate(_creatureDetailCardPrefab) as GameObject,
                CardType.Technology => Instantiate(_technologyDetailCardPrefab) as GameObject,
                CardType.Money => Instantiate(_moneyDetailCardPrefab) as GameObject,
                _ => throw new System.Exception("Card type not found")
            };

            detailCardObject.transform.SetParent(_grid, false);
            detailCardObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            // Initialize detail card
            var detailCard = detailCardObject.GetComponent<DetailCard>();
            detailCard.SetCardUI(cardInfo);
            detailCards.Add(detailCard);
        }
        
        return detailCards;
    }

    // public void SelectCard(Transform t) => t.SetParent(_gridChosen, false);
    // public void DeselectCard(Transform t) => t.SetParent(_gridAll, false);

    // public void ClearGrids(){
    //     foreach (Transform child in _gridAll) Destroy(child.gameObject);
    //     ClearChosenGrid();
    // }

    // public void ClearChosenGrid(){
    //     foreach (Transform child in _gridChosen) Destroy(child.gameObject);
    // }
}
