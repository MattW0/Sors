using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DropZoneManager : NetworkBehaviour
{
    private PlayerManager _player;
    public static DropZoneManager Instance { get; private set; }
    private BoardManager _boardManager;
    private CombatState _combatState;
    [SerializeField] private EntityZones entityZones;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    public static event Action OnStartAttacking;
    public static event Action OnStartBlocking;
    public static event Action OnCombatEnded;

    private void Start()
    {
        if (!Instance) Instance = this;
        _player = PlayerManager.GetLocalPlayer();

        if (isServer) _boardManager = BoardManager.Instance;
    }

    #region Entities ETB and LTB
    
    [Server]
    public void EntityEntersDropZone(PlayerManager owner, BattleZoneEntity entity)
    {
        if(entity.cardType == CardType.Development) {
            var development = entity.GetComponent<DevelopmentEntity>();
            entityZones.AddDevelopment(development, owner.isLocalPlayer);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.AddCreature(creature, owner.isLocalPlayer);
        }

        entityZones.RpcMoveEntityToHolder(entity);
        entity.OnDeath += EntityLeavesPlayZone;
    }

    [Server]
    private void EntityLeavesPlayZone(PlayerManager owner, BattleZoneEntity entity)
    {
        if(entity.cardType == CardType.Development) {
            var development = entity.GetComponent<DevelopmentEntity>();
            entityZones.RemoveDevelopment(development, owner.isLocalPlayer);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.RemoveCreature(creature, owner.isLocalPlayer);
        }

        entity.OnDeath -= EntityLeavesPlayZone;
    }

    #endregion

    #region Attackers

    [Server]
    public void StartDeclareAttackers(List<PlayerManager> players)
    {
        _combatState = CombatState.Attackers;
        foreach (var player in players) {
            var creatures = entityZones.GetCreatures(player.isLocalPlayer);
            TargetDeclareAttackers(player.connectionToClient, creatures);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn, List<CreatureEntity> creatures)
    {
        // Auto-skipping if player has empty board
        if (creatures.Count == 0) {
            if (isServer) PlayerFinishedChoosingAttackers(_player, true);
            else CmdPlayerSkipsAttackers(_player);
            return;
        }
        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        OnStartAttacking?.Invoke();
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerSkipsAttackers(PlayerManager player){
        PlayerFinishedChoosingAttackers(player, true);
    }

    [Server]
    private void PlayerFinishedChoosingAttackers(PlayerManager player, bool skip = false)
    {
        if (skip) {
            _boardManager.DisableReadyButton(player);
            _boardManager.AttackersDeclared(player, new List<CreatureEntity>());
            return;
        }

        var creaturesList = entityZones.GetCreatures(player.isLocalPlayer);
        TargetReturnAttackersList(player.connectionToClient, creaturesList, player);
    }

    [TargetRpc]
    private void TargetReturnAttackersList(NetworkConnection conn, List<CreatureEntity> creatures, PlayerManager player)
    {
        var attackers = new List<CreatureEntity>();
        foreach (var creature in creatures) {
            creature.LocalPlayerIsReady();
            if (!creature.IsAttacking) continue;

            attackers.Add(creature);
        }

        if (isServer) _boardManager.AttackersDeclared(player, attackers);
        else CmdReturnAttackersList(player, attackers);
    }

    [Command(requiresAuthority = false)]
    private void CmdReturnAttackersList(PlayerManager player, List<CreatureEntity> attackers) => _boardManager.AttackersDeclared(player, attackers);
    
    #endregion

    #region Blockers

    [Server]
    public void StartDeclareBlockers(List<PlayerManager> players)
    {
        _combatState = CombatState.Blockers;
        foreach (var player in players) {
            // Inverting isLocalPlayer because we want opponent creatures
            var opponentCreatures = entityZones.GetCreatures(!player.isLocalPlayer);
            var isAttacked = opponentCreatures.Exists(entity => entity.IsAttacking);
            TargetDeclareBlockers(player.connectionToClient, opponentCreatures, isAttacked);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<CreatureEntity> creatures, bool isAttacked)
    {
        var nbAttackers = 0;
        foreach (var creature in creatures) {
            if (creature.IsAttacking) nbAttackers++;
        }

        // If not being attacked or all my creatures are attacking, we skip
        // isAttacked is true if there is at least one attacking opponent entity
        if (!isAttacked || creatures.Count == nbAttackers) {
            PlayerSkipsBlockers();
            return;
        }

        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        OnStartBlocking?.Invoke();
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Blockers);
        }
    }

    private void PlayerSkipsBlockers(){
        if(isServer) PlayerFinishedChoosingBlockers(_player, true);
        else CmdPlayerSkipsBlockers(_player);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerSkipsBlockers(PlayerManager player){
        PlayerFinishedChoosingBlockers(player, true);
    }

    [Server]
    public void PlayerFinishedChoosingBlockers(PlayerManager player, bool skip = false)
    {
        _boardManager.DisableReadyButton(player);
        _boardManager.BlockersDeclared(player, new List<CreatureEntity>());
    }

    [Server]
    public void CombatCleanUp()
    {
        _combatState = CombatState.CleanUp;
        OnCombatEnded?.Invoke();
    }
    #endregion

    #region UI and utils

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

    [ClientRpc]
    public void RpcHighlightCardHolders(TurnState state) => entityZones.HighlightCardHolders(state);
    [ClientRpc]
    public void RpcResetHolders() => entityZones.ResetHolders();

    #endregion
}
