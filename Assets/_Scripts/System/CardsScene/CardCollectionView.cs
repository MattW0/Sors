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
        // startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);
        // DisplayCards(startEntities);

        moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");
        DisplayCards(moneyCardsDb);

        // var nbPaperMoney = 18;
        // var paperMones = new ScriptableCard[nbPaperMoney];
        // for (var i = 0; i < nbPaperMoney; i++)
        // {
        //     if (i < nbPaperMoney / 2)
        //         paperMones[i] = moneyCardsDb[0];
        //     else if (i < 14)
        //         paperMones[i] = moneyCardsDb[1];
        //     else 
        //         paperMones[i] = moneyCardsDb[2];
        // }
        // DisplayCards(paperMones);
        creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
        DisplayCards(creatureCardsDb);
        technologyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/TechnologyCards/");
        DisplayCards(technologyCardsDb);
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
