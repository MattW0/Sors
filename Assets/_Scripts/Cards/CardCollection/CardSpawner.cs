using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardGrid))]
public class CardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _creatureDetailCardPrefab;
    [SerializeField] private GameObject _technologyDetailCardPrefab;
    [SerializeField] private GameObject _moneyDetailCardPrefab;
    [SerializeField] private Transform _spawnParentTransform;
    private CardGrid _grid;

    private void Awake()
    {
        _grid = GetComponent<CardGrid>();
    }

    public void SpawnDetailCardObjectsInGrid(List<CardInfo> cards)
    {
        _grid.SetPanelDimension(cards.Count);

        var transforms = new List<Transform>();
        foreach (var cardInfo in cards){
            transforms.Add(InstantiateCard(cardInfo));
        }

        _grid.Add(transforms);
    }

    // Only for cards scene
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
        var detailCardObject = cardInfo.type switch
        {
            CardType.Creature => Instantiate(_creatureDetailCardPrefab),
            CardType.Technology => Instantiate(_technologyDetailCardPrefab),
            CardType.Money => Instantiate(_moneyDetailCardPrefab),
            _ => throw new System.Exception("Trying to spawn detail card with invalid type: " + cardInfo.type)
        };

        // Initialize detail card
        detailCardObject.GetComponent<DetailCardUI>().SetCardUI(cardInfo, cardInfo.cardSpritePath, true);

        return detailCardObject.transform;
    }
}
