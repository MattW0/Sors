using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayZoneManager : NetworkBehaviour
{
    public bool isMyZone;
    [SerializeField] private MoneyPlayZone moneyZone;
    private TurnManager _turnManager;
    private GameManager _gameManager;
    
    [SerializeField] private List<CardInfo> playedCardsList;

    private void Awake()
    {
        playedCardsList = new List<CardInfo>();
        if (isServer && !_gameManager) _gameManager = GameManager.Instance;
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
    
}
