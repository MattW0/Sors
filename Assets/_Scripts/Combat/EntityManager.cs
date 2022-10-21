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

    [SerializeField] private List<BattleZoneEntity> _battleZoneEntities;
    public List<BattleZoneEntity> GetEntities() => _battleZoneEntities;

    private void Awake()
    {
        _battleZoneEntities = new List<BattleZoneEntity>();
        BoardManager.OnEntityAdded += RpcEntityAdded;
    }

    public static void PlayerDeployCard(GameObject card, int holderNumber)
    {
        var networkIdentity = NetworkClient.connection.identity;
        var player = networkIdentity.GetComponent<PlayerManager>();
        
        player.CmdDeployCard(card, holderNumber);
    }
    
    [ClientRpc]
    private void RpcEntityAdded(BattleZoneEntity entity)
    {
        if (!entity.hasAuthority || !myZone) return;
        _battleZoneEntities.Add(entity);
    }
    
    [ClientRpc]
    public void RpcDeclareAttackers()
    {
        if (!myZone) return;
        
        foreach (var entity in _battleZoneEntities)
        {
            entity.CanAct(CombatState.Attackers);
        }
    }
    
    [ClientRpc]
    public void RpcDeclareBlockers()
    {
        if (!myZone) return;
        
        foreach (var entity in _battleZoneEntities)
        {
            entity.CanAct(CombatState.Blockers);
        }
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

    private void OnDestroy()
    {
        BoardManager.OnEntityAdded -= RpcEntityAdded;
    }
}
