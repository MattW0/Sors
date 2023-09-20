using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DropZoneManager : NetworkBehaviour
{
    public static DropZoneManager Instance { get; private set; }
    private PlayerManager _player;
    private CombatState _combatState;
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private EntityZones entityZones;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    public static event Action OnDestroyBlockerArrows;
    public static event Action OnDestroyTargetArrows;

    private void Awake(){
        if (!Instance) Instance = this;
    }
    private void Start() => _player = PlayerManager.GetLocalPlayer(); 

    #region Entities ETB and LTB
    
    [Server]
    public void EntityEntersDropZone(PlayerManager owner, BattleZoneEntity entity)
    {
        // print("Entering dropzone, owner: " + owner.name + ", card: " + entity.name);
        if(entity.cardType == CardType.Technology) {
            var development = entity.GetComponent<DevelopmentEntity>();
            entityZones.AddDevelopment(development, owner.isLocalPlayer);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.AddCreature(creature, owner.isLocalPlayer);
            creature.RpcRetreatAttacker();
        }

        entityZones.RpcMoveEntityToHolder(entity);
    }

    [Server]
    public void EntityLeavesPlayZone(BattleZoneEntity entity)
    {
        if(entity.cardType == CardType.Technology) {
            var development = entity.GetComponent<DevelopmentEntity>();
            entityZones.RemoveDevelopment(development, entity.Owner.isLocalPlayer);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.RemoveCreature(creature, entity.Owner.isLocalPlayer);
        }
    }

    #endregion

    [Server]
    public void ShowTargets(PlayerManager owner, EffectTarget target)
    {
        var entities = entityZones.GetAllEntities();
        print("Showing targets for player: " + owner.PlayerName + ", target: " + target);
        print("targets count: " + entities.Count);

        TargetMakeEntitiesTargetable(owner.connectionToClient, entities);
    }

    [TargetRpc]
    private void TargetMakeEntitiesTargetable(NetworkConnection conn, List<BattleZoneEntity> entities)
    {
        foreach (var entity in entities) {
            entity.Targetable = true;
        }
    }

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
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerSkipsAttackers(PlayerManager player) => PlayerFinishedChoosingAttackers(player, true);

    [Server]
    private void PlayerFinishedChoosingAttackers(PlayerManager player, bool skip = false)
    {
        _boardManager.DisableReadyButton(player);
        
        if (skip) {
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
        else CmdAttackersDeclared(player, attackers);
    }

    [Command(requiresAuthority = false)]
    private void CmdAttackersDeclared(PlayerManager player, List<CreatureEntity> attackers) => _boardManager.AttackersDeclared(player, attackers);
    
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

            var playerCreatures = entityZones.GetCreatures(player.isLocalPlayer);
            TargetDeclareBlockers(player.connectionToClient, playerCreatures, isAttacked);
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
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Blockers);
        }
    }

    private void PlayerSkipsBlockers()
    {
        if(isServer) PlayerFinishedChoosingBlockers(_player, true);
        else CmdPlayerSkipsBlockers(_player);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerSkipsBlockers(PlayerManager player) => PlayerFinishedChoosingBlockers(player, true);

    [Server]
    public void PlayerFinishedChoosingBlockers(PlayerManager player, bool skip = false)
    {
        _boardManager.DisableReadyButton(player);
        _boardManager.BlockersDeclared(player);
    }

    [Server]
    public void CombatCleanUp()
    {
        _combatState = CombatState.CleanUp;
        var creatures = entityZones.GetAllCreatures();
        foreach (var creature in creatures){

            creature.RpcResetAfterCombat();
        }

        RpcDestroyBlockerArrows();
    }

    [ClientRpc]
    private void RpcDestroyBlockerArrows() => OnDestroyBlockerArrows?.Invoke();
    #endregion

    // TODO: Check if it makes sense to loose health after triggering or at the end of the turn
    // How to handle developments without trigger ? 
    [Server]
    public void DevelopmentsLooseHealth()
    {
        var developments = entityZones.GetAllDevelopments();

        foreach(var development in developments){
            var entity = development.gameObject.GetComponent<BattleZoneEntity>();
            entity.Health -= 1;
        }
    }

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

    [Server]
    public void DestroyTargetArrows()
    {
        var entities = entityZones.GetAllEntities();
        foreach (var entity in entities) entity.RpcResetAfterTarget();

        RpcDestroyTargetArrows();
    }

    [ClientRpc]
    private void RpcDestroyTargetArrows() => OnDestroyTargetArrows?.Invoke();

    [ClientRpc]
    public void RpcHighlightCardHolders(TurnState state) => entityZones.HighlightCardHolders(state);
    
    [ClientRpc]
    public void RpcResetHolders() => entityZones.ResetHolders();

    #endregion
}
