using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerZoneManager : NetworkBehaviour
{
    [SerializeField] private bool myZone;
    [SerializeField] private BattleZone battleZone;
    [SerializeField] private MoneyZone moneyZone;
    [SerializeField] private List<BattleZoneEntity> _battleZoneEntities;
    private BoardManager _boardManager;
    private PlayerManager _zoneOwner;

    private void Awake(){
        _battleZoneEntities = new List<BattleZoneEntity>();
        BoardManager.OnEntityAdded += RpcEntityEntersPlayZone;
    }

    private void Start(){        
        var networkIdentity = NetworkClient.connection.identity;
        _zoneOwner = networkIdentity.GetComponent<PlayerManager>();

        if (isServer) _boardManager = BoardManager.Instance;
    }

    #region Entities

    public static void PlayerDeployCard(GameObject card, int holderNumber)
    {
        var networkIdentity = NetworkClient.connection.identity;
        var player = networkIdentity.GetComponent<PlayerManager>();
        
        player.CmdDeployCard(card, holderNumber);
    }
    
    [ClientRpc]
    private void RpcEntityEntersPlayZone(BattleZoneEntity entity){
        if (!entity.isOwned || !myZone) return;

        _battleZoneEntities.Add(entity);
        entity.OnDeath += RpcEntityLeavesPlayZone;
    }

    [ClientRpc]
    private void RpcEntityLeavesPlayZone(BattleZoneEntity entity){
        if (!entity.isOwned || !myZone) return;

        _battleZoneEntities.Remove(entity);
        entity.OnDeath -= RpcEntityLeavesPlayZone;
    }
    
    [ClientRpc]
    public void RpcDeclareAttackers()
    {
        // Auto-skipping if player has empty board
        if (_battleZoneEntities.Count == 0) {
            print("Auto-skip attackers");
            if (isServer) _boardManager.PlayerSkipsCombatPhase(_zoneOwner);
            else CmdSkipCombatPhase(_zoneOwner);
            return;
        }

        foreach (var entity in _battleZoneEntities)
        {
            entity.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [TargetRpc]
    public void TargetPlayerFinishedChoosingAttackers(NetworkConnection conn)
    {
        foreach (var entity in _battleZoneEntities)
        {
            if (entity.IsAttacking) {
                entity.IsAttacker();
                print(entity.Title + " is attacking");
            }

            else entity.CanNotAct();
        }
    }

    public int GetAttackersCount()
    {
        var count = 0;
        foreach (var entity in _battleZoneEntities)
        {
            if (entity.IsAttacking) count++;
        }

        return count;
    }
    
    [ClientRpc]
    public void RpcDeclareBlockers(int opponentAttackers)
    {
        var hasBlocker = false;
        foreach (var entity in _battleZoneEntities){
            entity.CheckIfCanAct(CombatState.Blockers);

            if (entity.IsAttacking) continue;
            hasBlocker = true;
        }

        // Auto-skipping if player has empty board or no blockers
        if (!hasBlocker || opponentAttackers == 0){
            print("Auto-skip blockers");
            if (isServer) _boardManager.PlayerSkipsCombatPhase(_zoneOwner);
            else CmdSkipCombatPhase(_zoneOwner);
            return;
        }
    }

    [ClientRpc]
    public void RpcCombatCleanUp(){
        foreach (var entity in _battleZoneEntities){
            entity.ResetAfterCombat();
        }
    }

    #endregion

    #region UI and utils

    [ClientRpc]
    public void RpcHighlightCardHolders(bool active)
    {
        battleZone.Highlights(active);
    }
    
    [ClientRpc]
    public void RpcResetHolders()
    {
        battleZone.ResetHighlights();
    }
    
    [ClientRpc]
    public void RpcDiscardMoney()
    {
        var cards = moneyZone.GetCards();
        foreach (var card in cards) {
            card.GetComponent<CardMover>().MoveToDestination(myZone, CardLocations.Discard);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSkipCombatPhase(PlayerManager owner) => _boardManager.PlayerSkipsCombatPhase(owner);

    #endregion

    private void OnDestroy()
    {
        BoardManager.OnEntityAdded -= RpcEntityEntersPlayZone;
    }
}
