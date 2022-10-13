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
    
    private void Awake()
    {
        if (!isMyZone) return;

        CombatManager.OnDeclareAttackers += RpcDeclareAttackers;
        CombatManager.OnDeclareBlockers += RpcDeclareBlockers;
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        foreach (var card in cards) {
            card.GetComponent<CardMover>().MoveToDestination(isMyZone, CardLocations.Discard, -1);
        }
    }
    
    [ClientRpc]
    public void RpcShowCardPositionOptions(bool active)
    {
        if (!isMyZone) return;
        
        // Resetting at end of Deploy phase 
        if (!active) {
            battleZone.ResetHighlight();
        } else {
            battleZone.HighlightCardHolders();
        }
    }
    
    public static void DeployCard(GameObject card, int holderNumber)
    {
        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDeployCard(card, holderNumber);
    }
    
    [ClientRpc]
    private void RpcDeclareAttackers() {
        if (!isMyZone) return;
        print("Declare attackers!");
        
        print(battleZone.GetCards.Count + " cards on field");
    }

    [ClientRpc]
    private void RpcDeclareBlockers() {
        if (!isMyZone) return;
        print("Declare blockers!");
    }
    
    private void OnDestroy()
    {
        CombatManager.OnDeclareAttackers -= RpcDeclareAttackers;
        CombatManager.OnDeclareBlockers -= RpcDeclareBlockers;
    }
}
