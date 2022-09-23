using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class PlayZoneManager : NetworkBehaviour
{
    public bool isMyZone;
    [SerializeField] private MoneyZone moneyZone;
    [SerializeField] private BattleZone battleZone;
    
    private TurnManager _turnManager;
    private GameManager _gameManager;
    
    [SerializeField] private Dictionary<int, CardStats> playedCardsList;

    public void Prepare()
    {
        if (isServer && !_gameManager) _gameManager = GameManager.Instance;
        
        battleZone.Prepare();
        playedCardsList = new Dictionary<int, CardStats>();
        var nbCardHolders = battleZone.GetNbCardHolders();
        for (int i = 0; i < nbCardHolders; i++)
        {
            playedCardsList.Add(i, null); // empty battlezone
        }
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        
        foreach (var card in cards)
        {
            card.GetComponent<CardMover>().MoveToDestination(isMyZone, CardLocations.Discard);
        }
    }

    public void DeployToBattlezone()
    {
        ShowCardPositionOptions();
    }

    private void ShowCardPositionOptions()
    {
        List<int> freeIds = new List<int>();
        foreach (var (i, card) in playedCardsList)
        {
            if (card) continue;
            
            freeIds.Add(i);
        }
        
        battleZone.HighlightCardHolders(freeIds.ToArray(), true);
    }
}
