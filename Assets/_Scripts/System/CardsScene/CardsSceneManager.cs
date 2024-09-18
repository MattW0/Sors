using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsSceneManager : MonoBehaviour
{
    private CardSpawner _cardSpawner;
    private CardsSceneUI _cardsSceneUI;
    [SerializeField] private CardCreateWindow _cardCreateWindow;
    public List<CardInfo> startDeck = new();
    public List<CardInfo> moneyCards = new();
    public List<CardInfo> creatureCards = new();
    public List<CardInfo> technologyCards = new();
    private Dictionary<CardType, List<GameObject>> _detailCardObjects = new();
    private const int DECK_SIZE = 10;

    private void Awake()
    {
        _cardSpawner = GetComponent<CardSpawner>();
        _cardsSceneUI = GetComponent<CardsSceneUI>();
    }

    private void Start()
    {
        startDeck = LoadCards("Cards/_StartCards/");
        moneyCards = LoadCards("Cards/MoneyCards/");
        creatureCards = LoadCards("Cards/CreatureCards/");
        technologyCards = LoadCards("Cards/TechnologyCards/");

        foreach(var c in startDeck) {
            if (c.type == CardType.Creature) creatureCards.Insert(0, c);
            else technologyCards.Insert(0, c);
        }
        for (int i=startDeck.Count; i<DECK_SIZE; i++) startDeck.Add(moneyCards[0]);

        _detailCardObjects.Add(CardType.Player, _cardSpawner.SpawnDetailCardObjects(startDeck));
        _detailCardObjects.Add(CardType.Money, _cardSpawner.SpawnDetailCardObjects(moneyCards));
        _detailCardObjects.Add(CardType.Creature, _cardSpawner.SpawnDetailCardObjects(creatureCards));
        _detailCardObjects.Add(CardType.Technology, _cardSpawner.SpawnDetailCardObjects(technologyCards));
    }

    private List<CardInfo> LoadCards(string resourcePath)
    {
        var cardInfos = new List<CardInfo>();
        foreach (var s in Resources.LoadAll<ScriptableCard>(resourcePath))
        {
            cardInfos.Add(new CardInfo(s, -1));
        }

        return cardInfos;
    }

    public void SelectCardType(CardType type)
    {
        foreach (var (k,v) in _detailCardObjects) {
            foreach (var go in v) 
                go.SetActive(type == CardType.All || k == type);
        }

        _cardsSceneUI.ScrollToTop();
    }

    internal void OpenCardCreateWindow()
    {
        _cardCreateWindow.ModalWindowIn();
    }
}
