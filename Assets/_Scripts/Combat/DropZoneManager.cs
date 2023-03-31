using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DropZoneManager : NetworkBehaviour
{
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    [SerializeField] private PlayZoneCardHolder[] playerCardHolders = new PlayZoneCardHolder[6];
    [SerializeField] private PlayZoneCardHolder[] opponentCardHolders = new PlayZoneCardHolder[6];
    [SerializeField] private List<BattleZoneEntity> _hostEntities = new();
    [SerializeField] private List<BattleZoneEntity> _clientEntities = new();
    private BoardManager _boardManager;
    private PlayerManager _player;
    private CombatState _combatState;

    private void Start(){    
        if (isServer) _boardManager = BoardManager.Instance;

        _player = PlayerManager.GetLocalPlayer();
    }

    #region Entities
    
    [Server]
    public void EntityEntersDropZone(PlayerManager owner, BattleZoneEntity entity){

        var index = 0;
        if(owner.isLocalPlayer) {
            _hostEntities.Add(entity);
            index = _hostEntities.Count - 1;
        } else {
            _clientEntities.Add(entity);
            index = _clientEntities.Count - 1;
        }

        RpcMoveEntityToHolder(entity, index);
        ResetHolders();
        entity.OnDeath += EntityLeavesPlayZone;
    }

    [ClientRpc]
    private void RpcMoveEntityToHolder(BattleZoneEntity entity, int index){
        if(entity.isOwned) entity.transform.SetParent(playerCardHolders[index].transform, false);
        else entity.transform.SetParent(opponentCardHolders[index].transform, false);
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
    print(_player.PlayerName +" has "+ entities.Count + " entities");
        // Auto-skipping if player has empty board
        if (entities.Count == 0) {
            if (isServer) PlayerFinishedChoosingAttackers(_player, true);
            else CmdPlayerFinishedChoosingAttackers(_player, true);
            return;
        }
        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        foreach (var entity in entities) {
            entity.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerFinishedChoosingAttackers(PlayerManager player, bool skip){
        PlayerFinishedChoosingAttackers(player, skip);
    }

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
    public void RpcHighlightCardHolders(bool active){
        if (active) HighlightHolders();
        else ResetHolders();
    }

    public void HighlightHolders()
    {
        foreach (var holder in playerCardHolders) {
            holder.SetHighlight();
        }
    }
    
    public void ResetHolders()
    {
        foreach (var holder in playerCardHolders) {
            holder.ResetHighlight();
        }
    }

    [ClientRpc]
    public void RpcUndoPlayMoney() {
        // if(isClient)
        print("dropzone: undo");   
        playerMoneyZone.UndoPlayMoney();
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
}
