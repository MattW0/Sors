using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCollectionView : MonoBehaviour
{
    private CardSpawner _cardSpawner; 
    public ScriptableCard[] startEntities;
    public ScriptableCard[] creatureCardsDb;
    public ScriptableCard[] moneyCardsDb;
    public ScriptableCard[] technologyCardsDb;

    private void Awake()
    {
        _cardSpawner = GetComponent<CardSpawner>();
        LoadCards();
    }

    private void LoadCards()
    {
        startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");
        DisplayCards(startEntities);
        moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");
        DisplayCards(moneyCardsDb);
        creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
        DisplayCards(creatureCardsDb);
        technologyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/TechnologyCards/");
        DisplayCards(technologyCardsDb);
    }

    private void RepeatStartEntities()
    {
        startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");
        for (int i=0; i<5; i++) DisplayCards(startEntities);
    }

    private void RepeatMoneyCards()
    {
        var nb = 18;
        var paperMones = new ScriptableCard[nb];
        for (var i = 0; i < nb; i++)
        {
            if (i < nb / 2)
                paperMones[i] = moneyCardsDb[0];
            else if (i < nb - 5)
                paperMones[i] = moneyCardsDb[1];
            else 
                paperMones[i] = moneyCardsDb[2];
        }
        DisplayCards(paperMones);
    }

    private void DisplayCards(ScriptableCard[] scriptables)
    {
        var cardInfos = new List<CardInfo>();
        foreach (var s in scriptables)
        {
            cardInfos.Add(new CardInfo(s, -1));
        }
        _cardSpawner.SpawnDetailCardObjects(cardInfos, TurnState.Idle, 0.95f);
    }
}
