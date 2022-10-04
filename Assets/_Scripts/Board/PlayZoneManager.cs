using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayZoneManager : NetworkBehaviour
{
    public bool isMyZone;
    [SerializeField] private MoneyZone moneyZone;
    [SerializeField] private BattleZone battleZone;
    
    // private TurnManager _turnManager;
    // private GameManager _gameManager;
    
    private Dictionary<int, CardStats> _battleZoneCards;

    private void Awake()
    {
        if (!isMyZone) return;
        
        _battleZoneCards = new Dictionary<int, CardStats>();
        
        // Disgusting but allows for easy extension
        var nbCardHolders = battleZone.GetNbCardHolders();
        for (var i = 0; i < nbCardHolders; i++)
        {
            _battleZoneCards.Add(i, null); // empty battlezone
        }
        
        PlayZoneCardHolder.OnCardDeployed += HandleCardDeployed;
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        
        foreach (var card in cards)
        {
            card.GetComponent<CardMover>().MoveToDestination(isMyZone, CardLocations.Discard, -1);
        }
    }
    
    // gruuuuusig
    [ClientRpc]
    public void RpcShowCardPositionOptions(bool active)
    {
        if (!isMyZone) return;
        
        // Resetting at end of Deploy phase 
        if (!active)
        {
            battleZone.ResetHighlight();
            return;
        }
        
        var freeIds = new List<int>();
        foreach (var (i, card) in _battleZoneCards)
        {
            if (card) continue;
            
            freeIds.Add(i);
        }
        
        battleZone.HighlightCardHolders(freeIds.ToArray(), true);
    }

    private void HandleCardDeployed(GameObject card, int holderNumber)
    {
        var cardStats = card.GetComponent<CardStats>();
        cardStats.IsDeployable = false;
        _battleZoneCards[holderNumber] = cardStats;
        
        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDeployCard(card, holderNumber);
    }

    private void OnDestroy()
    {
        PlayZoneCardHolder.OnCardDeployed -= HandleCardDeployed;
    }
}
