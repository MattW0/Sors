using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DropZoneManager : NetworkBehaviour
{
    [SerializeField] private BattleZone battleZone;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    [SerializeField] private List<BattleZoneEntity> _hostEntities = new();
    [SerializeField] private List<BattleZoneEntity> _clientEntities = new();
    private BoardManager _boardManager;
    private PlayerManager _player;
    private CombatState _combatState;

    private void Awake(){
        BoardManager.OnEntityAdded += EntityEntersPlayZone;
    }

    private void Start(){    
        if (isServer) _boardManager = BoardManager.Instance;

        _player = NetworkClient.connection.identity.GetComponent<PlayerManager>();
    }

    #region Entities

    public static void PlayerDeployCard(GameObject card, int holderNumber)
    {
        var networkIdentity = NetworkClient.connection.identity;
        var player = networkIdentity.GetComponent<PlayerManager>();
        
        player.CmdDeployCard(card, holderNumber);
    }
    
    [Server]
    private void EntityEntersPlayZone(PlayerManager owner, BattleZoneEntity entity){

        if(owner.isLocalPlayer) _hostEntities.Add(entity);
        else _clientEntities.Add(entity);

        entity.OnDeath += EntityLeavesPlayZone;
    }

    [Server]
    private void EntityLeavesPlayZone(PlayerManager owner, BattleZoneEntity entity){
        if(owner.isLocalPlayer) _hostEntities.Remove(entity);
        else _clientEntities.Remove(entity);

        entity.OnDeath -= EntityLeavesPlayZone;
    }

    #endregion

    #region Attackers

    [Server]
    public void PlayersDeclareAttackers(List<PlayerManager> players){
        _combatState = CombatState.Attackers;
        foreach (var player in players) {
            if (player.isLocalPlayer) TargetDeclareAttackers(player.connectionToClient, _hostEntities);
            else TargetDeclareAttackers(player.connectionToClient, _clientEntities);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn, List<BattleZoneEntity> entities)
    {
        // Auto-skipping if player has empty board
        if (entities.Count == 0) {
            if (isServer) PlayerFinishedChoosingAttackers(_player, true);
            else CmdPlayerFinishedChoosingAttackers(true);
            return;
        }
        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        foreach (var entity in entities) {
            entity.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerFinishedChoosingAttackers(bool skip) => PlayerFinishedChoosingAttackers(_player, skip);

    [Server]
    private void PlayerFinishedChoosingAttackers(PlayerManager player, bool skip = false)
    {
        if (skip) {
            _boardManager.DisableReadyButton(player);
            _boardManager.AttackersDeclared(player, new List<BattleZoneEntity>());
            return;
        }

        List<BattleZoneEntity> entitiesList = new();
        if (player.isLocalPlayer) entitiesList = _hostEntities;
        else entitiesList = _clientEntities;

        TargetReturnAttackersList(player.connectionToClient, entitiesList, player);
    }

    [TargetRpc]
    private void TargetReturnAttackersList(NetworkConnection conn, List<BattleZoneEntity> entities, PlayerManager player)
    {
        var attackers = new List<BattleZoneEntity>();
        foreach (var entity in entities) {
            entity.LocalPlayerIsReady();
            if (!entity.IsAttacking) continue;

            attackers.Add(entity);
        }

        if (isServer) _boardManager.AttackersDeclared(player, attackers);
        else CmdReturnAttackersList(player, attackers);
    }

    [Command(requiresAuthority = false)]
    private void CmdReturnAttackersList(PlayerManager player, List<BattleZoneEntity> attackers) => _boardManager.AttackersDeclared(player, attackers);
    
    #endregion

    #region Blockers

    [Server]
    public void PlayersDeclareBlockers(List<PlayerManager> players){
        _combatState = CombatState.Blockers;
        foreach (var player in players) {
            // Host Logic
            if (player.isLocalPlayer) {
                // Is there an opponent creature attacking? 
                // isAttacked is true if _clientEntities has at least one entity with IsAttacking == true
                var isHostAttacked = _clientEntities.Exists(entity => entity.IsAttacking);
                TargetDeclareBlockers(player.connectionToClient, _hostEntities, isHostAttacked);
                return;
            }
            
            // Client Logic
            var isClientAttacked = _hostEntities.Exists(entity => entity.IsAttacking);
            TargetDeclareBlockers(player.connectionToClient, _clientEntities, isClientAttacked);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<BattleZoneEntity> entities, bool isAttacked)
    {
        var nbAttackers = 0;
        foreach (var entity in entities) {
            if (entity.IsAttacking) nbAttackers++;
        }

        // If not being attacked or all my creatures are attacking, we skip
        if (!isAttacked || entities.Count == nbAttackers) {
            if(isServer) {
                _boardManager.DisableReadyButton(_player);
                PlayerFinishedChoosingBlockers(_player, true);
            }
            else CmdPlayerFinishedChoosingBlockers(_player, true);
            return;
        }
        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        foreach (var entity in entities) {
            entity.CheckIfCanAct(CombatState.Blockers);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerFinishedChoosingBlockers(PlayerManager player, bool skip){
        _boardManager.DisableReadyButton(player);
        PlayerFinishedChoosingBlockers(player, skip);
    }

    [Server]
    public void PlayerFinishedChoosingBlockers(PlayerManager player, bool skip = false)
    {
        _boardManager.BlockersDeclared(player, new List<BattleZoneEntity>());
    }

    [Server]
    public void CombatCleanUp(){
        var entities = new List<BattleZoneEntity>();
        entities.AddRange(_hostEntities);
        entities.AddRange(_clientEntities);
        RpcResetCreatures(entities);
    }

    [ClientRpc]
    private void RpcResetCreatures(List<BattleZoneEntity> entities){
        foreach (var entity in entities){
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
        playerMoneyZone.DiscardMoney();
        opponentMoneyZone.DiscardMoney();
    }

    [Server]
    public void PlayerPressedReadyButton(PlayerManager player)
    {
        if (_combatState == CombatState.Attackers) {
            PlayerFinishedChoosingAttackers(player);
        }
        else if (_combatState == CombatState.Blockers) {
            PlayerFinishedChoosingBlockers(player);
        }
    }

    #endregion

    private void OnDestroy()
    {
        BoardManager.OnEntityAdded -= EntityEntersPlayZone;
    }
}
