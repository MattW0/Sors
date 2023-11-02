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
    public static event Action OnDestroyArrows;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    private void Start() => _player = PlayerManager.GetLocalPlayer();

    #region Entities ETB and LTB

    [Server]
    public void EntityEntersDropZone(PlayerManager owner, BattleZoneEntity entity)
    {
        // print("Entering dropzone, owner: " + owner.name + ", card: " + entity.name);
        if (entity.cardType == CardType.Technology)
        {
            var development = entity.GetComponent<TechnologyEntity>();
            entityZones.AddDevelopment(development, owner.isLocalPlayer);
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.AddCreature(creature, owner.isLocalPlayer);
        }

        entityZones.RpcMoveEntityToHolder(entity);
    }

    [Server]
    public void EntityLeavesPlayZone(BattleZoneEntity entity)
    {
        if (entity.cardType == CardType.Technology)
        {
            var development = entity.GetComponent<TechnologyEntity>();
            entityZones.RemoveDevelopment(development, entity.Owner.isLocalPlayer);
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.RemoveCreature(creature, entity.Owner.isLocalPlayer);
        }
    }

    #endregion

    [Server]
    public void ShowAbilityTargets(PlayerManager owner, EffectTarget target)
    {
        // TODO: Expand for different possible effect targets and standard combat targeting
        var entities = entityZones.GetAllEntities();
        print("Showing targets for player: " + owner.PlayerName + ", target: " + target);
        print("targets count: " + entities.Count);

        TargetMakeEntitiesTargetable(owner.connectionToClient, entities);
    }

    [TargetRpc]
    private void TargetMakeEntitiesTargetable(NetworkConnection conn, List<BattleZoneEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsTargetable = true;
        }
    }

    #region Attackers

    [Server]
    public void StartDeclareAttackers(List<PlayerManager> players)
    {
        _combatState = CombatState.Attackers;
        foreach (var player in players)
        {
            // Auto-skipping if local player has no creatures
            var creatures = entityZones.GetCreatures(player.isLocalPlayer);
            if (creatures.Count == 0) {
                PlayerFinishedChoosingAttackers(_player);
                return;
            }

            // Get opponent technologies
            var targets = entityZones.GetTechnologies(!player.isLocalPlayer);

            // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
            TargetDeclareAttackers(player.connectionToClient, creatures, targets);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn, List<CreatureEntity> creatures, List<BattleZoneEntity> targets)
    {
        foreach (var c in creatures) c.CheckIfCanAct();
        foreach (var t in targets) t.IsTargetable = true;
    }

    [Server]
    private void PlayerFinishedChoosingAttackers(PlayerManager player)
    {
        // From skip or pressing combat button
        _boardManager.DisableReadyButton(player);
        _boardManager.AttackersDeclared(player, new List<CreatureEntity>());

    }

    #endregion

    #region Blockers

    [Server]
    public void StartDeclareBlockers(List<PlayerManager> players)
    {
        _combatState = CombatState.Blockers;
        foreach (var player in players)
        {
            // Inverting isLocalPlayer because we want opponent creatures
            var opponentCreatures = entityZones.GetCreatures(!player.isLocalPlayer);
            var isAttacked = opponentCreatures.Exists(entity => entity.IsAttacking);

            print("isAttacked " + isAttacked);
            // foreach (var c in opponentCreatures)
            // {
            //     print(c.IsAttacking);
            // }
            var playerCreatures = entityZones.GetCreatures(player.isLocalPlayer);
            TargetDeclareBlockers(player.connectionToClient, playerCreatures, isAttacked);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<CreatureEntity> creatures, bool isAttacked)
    {
        var nbAttackers = 0;
        foreach (var creature in creatures)
        {
            if (creature.IsAttacking) nbAttackers++;
        }

        // If not being attacked or all my creatures are attacking, we skip
        // isAttacked is true if there is at least one attacking opponent entity
        if (!isAttacked || creatures.Count == nbAttackers)
        {
            print($"Skipping blockers");
            PlayerSkipsBlockers();
            return;
        }

        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        foreach (var creature in creatures)
        {
            print($"{creature.Title} check if can act");
            creature.CheckIfCanAct();
        }
    }

    private void PlayerSkipsBlockers()
    {
        if (isServer) PlayerFinishedChoosingBlockers(_player, true);
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

    #endregion

    [Server]
    public void CombatCleanUp()
    {
        _combatState = CombatState.CleanUp;
        var creatures = entityZones.GetAllCreatures();
        foreach (var creature in creatures)
        {

            creature.RpcResetAfterCombat();
        }

        RpcDestroyBlockerArrows();
    }

    // TODO: Check if it makes sense to loose health after triggering or at the end of the turn
    // How to handle developments without trigger ? 
    [Server]
    public void DevelopmentsLooseHealth()
    {
        var developments = entityZones.GetAllDevelopments();

        foreach (var development in developments)
        {
            development.Health -= 1;
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
        if (_combatState == CombatState.Attackers)
        {
            PlayerFinishedChoosingAttackers(player);
        }
        else if (_combatState == CombatState.Blockers)
        {
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
    private void RpcDestroyTargetArrows() => OnDestroyArrows?.Invoke();
    
    [ClientRpc]
    private void RpcDestroyBlockerArrows() => OnDestroyArrows?.Invoke();

    [ClientRpc]
    public void RpcHighlightCardHolders(TurnState state) => entityZones.HighlightCardHolders(state);

    [ClientRpc]
    public void RpcResetHolders() => entityZones.ResetHolders();

    #endregion
}
