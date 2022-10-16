using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EntityManager : NetworkBehaviour
{
    [SerializeField] private bool myZone;
    [SerializeField] private BattleZone battleZone;
    [SerializeField] private MoneyZone moneyZone;

    private List<BattleZoneEntity> _battleZoneEntities;
    public List<BattleZoneEntity> GetEntities() => _battleZoneEntities;

    private void Awake()
    {
        _battleZoneEntities = new List<BattleZoneEntity>();
    }

    public static void PlayerDeployCard(GameObject card, int holderNumber)
    {
        var networkIdentity = NetworkClient.connection.identity;
        var player = networkIdentity.GetComponent<PlayerManager>();
        
        player.CmdDeployCard(card, holderNumber);
    }

    [ClientRpc]
    public void RpcHighlight(bool active)
    {
        battleZone.Highlight(active);
    }
    
    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        foreach (var card in cards) {
            card.GetComponent<CardMover>().MoveToDestination(myZone, CardLocations.Discard);
        }
    }
}
