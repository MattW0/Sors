using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayZoneManager : NetworkBehaviour
{
    public bool isMyZone;
    [SerializeField] private MoneyPlayZone moneyZone;
    
    [Command]
    private void CmdUpdateCardCollection(PlayerManager owner, GameObject card)
    {
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        owner.cards.hand.Remove(cardInfo);
        owner.cards.discard.Add(cardInfo);
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        
        foreach (var card in cards)
        {
            card.GetComponent<CardMover>().MoveToDestination(isMyZone, CardLocations.Discard);
            // if (isMyZone) CmdUpdateCardCollection(owner, card);
        }
    }
    
}
