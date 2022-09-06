using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayZoneManager : NetworkBehaviour
{
    public PlayerManager zoneOwner;
    [SerializeField] private MoneyPlayZone moneyZone;
    
    public void PreparePlayZones()
    {
        zoneOwner = NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>();
    }

    public void DiscardMoneyCards(bool isZoneOwner)
    {
        moneyZone.DiscardMoney(isZoneOwner);
    }
}
