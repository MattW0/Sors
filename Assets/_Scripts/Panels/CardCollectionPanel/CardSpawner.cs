using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _creatureDetailCardPrefab;
    [SerializeField] private GameObject _technologyDetailCardPrefab;
    [SerializeField] private GameObject _moneyDetailCardPrefab;
    [SerializeField] private Transform _gridAll;
    [SerializeField] private Transform _gridChosen;

    public DetailCard SpawnDetailCardObject(CardInfo cardInfo){
        var detailCardObject = cardInfo.type switch{
            CardType.Creature => Instantiate(_creatureDetailCardPrefab) as GameObject,
            CardType.Technology => Instantiate(_technologyDetailCardPrefab) as GameObject,
            CardType.Money => Instantiate(_moneyDetailCardPrefab) as GameObject,
            _ => throw new System.Exception("Card type not found")
        };

        // Initialize detail card
        var detailCard = detailCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);

        return detailCard;
    }

    public void PlaceCard(GameObject detailCardObject){
        detailCardObject.transform.SetParent(_gridAll, false);
        detailCardObject.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        detailCardObject.transform.localPosition = new Vector3(0, -25f, 0);
    }

    public void ClearGrids(){
        foreach (Transform child in _gridAll) Destroy(child.gameObject);
        ClearChosenGrid();
    }

    public void ClearChosenGrid(){
        foreach (Transform child in _gridChosen) Destroy(child.gameObject);
    }
}
