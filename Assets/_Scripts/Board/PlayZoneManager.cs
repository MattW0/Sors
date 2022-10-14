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

    [SerializeField] private CombatManager combatManager;
    public List<BattleZoneEntity> Attackers { get; private set; }

    private void Awake()
    {
        if (!isMyZone) return;

        CombatManager.OnDeclareAttackers += RpcDeclareAttackers;
        CombatManager.OnDeclareBlockers += RpcDeclareBlockers;
        BattleZoneEntity.Attack += AttackerDeclared;

        Attackers = new List<BattleZoneEntity>();
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
        var player = networkIdentity.GetComponent<PlayerManager>();
        
        player.CmdDeployCard(card, holderNumber);
    }
    
    [ClientRpc]
    private void RpcDeclareAttackers() {
        if (!isMyZone) return;
        print("Declare attackers!");

        var entities = battleZone.GetEntities;
        print(entities.Count + " cards on field");
        if (entities.Count == 0)
        {
            combatManager.CmdPlayerSkipsPhase();
            return;
        }

        foreach (var entity in entities)
        {
            entity.CanAct(CombatState.Attackers);
        }
    }
    
    private void AttackerDeclared(BattleZoneEntity attacker, bool adding)
    {
        print("Adding attacker "+ attacker.Title + ": " + adding);
        
        if (!adding)
        {
            Attackers.Remove(attacker);
            return;
        }
        
        Attackers.Add(attacker);
    }

    [ClientRpc]
    private void RpcDeclareBlockers() {
        if (!isMyZone) return;
        print("Declare blockers!");
        
        var entities = battleZone.GetEntities;
        foreach (var entity in entities)
        {
            entity.CanAct(CombatState.Blockers);
        }
    }
    
    private void OnDestroy()
    {
        CombatManager.OnDeclareAttackers -= RpcDeclareAttackers;
        CombatManager.OnDeclareBlockers -= RpcDeclareBlockers;
        BattleZoneEntity.Attack -= AttackerDeclared;
    }
}
