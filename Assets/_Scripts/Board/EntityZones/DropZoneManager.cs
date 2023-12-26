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
    public IEnumerator EntitiesEnterDropZone(Dictionary<GameObject, BattleZoneEntity> entities)
    {   
        foreach (var (card, entity) in entities){
            // Need to await RPC for initialization
            while (!entity.Owner) yield return new WaitForSeconds(0.1f);
            var owner = entity.Owner;

            // To show which card spawns an entity 
            owner.RpcMoveCard(card, CardLocation.Hand, CardLocation.EntitySpawn);
            entityZones.RpcMoveEntityToSpawned(entity);

            // Keep track of entity objects for combat interactions
            entityZones.AddEntity(entity, owner.isLocalPlayer);

            // Spawning animation
            yield return new WaitForSeconds(SorsTimings.showSpawnedEntity);
            entityZones.RpcMoveEntityToHolder(entity);
            owner.RpcMoveCard(card, CardLocation.EntitySpawn, CardLocation.PlayZone);
        }
    }

    [Server]
    public void EntityLeavesPlayZone(BattleZoneEntity entity)
    {
        if (entity.CardType == CardType.Technology)
        {
            var development = entity.GetComponent<TechnologyEntity>();
            entityZones.RemoveDevelopment(development, entity.Owner.isLocalPlayer);
        }
        else if (entity.CardType == CardType.Creature)
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

        TargetMakeEntitiesTargetable(owner.connectionToClient, entities, true);
    }

    #region Attackers

    [Server]
    public void StartDeclareAttackers(PlayerManager player, BattleZoneEntity opponent)
    {
        // Auto-skipping if local player has no creatures
        var creatures = entityZones.GetCreatures(player.isLocalPlayer);
        if (creatures.Count == 0) {
            PlayerFinishedChoosingAttackers(_player);
            return;
        }

        // Get opponent attackable targets
        var targets = entityZones.GetTechnologies(!player.isLocalPlayer);
        if(opponent) targets.Add(opponent);

        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        TargetDeclareAttackers(player.connectionToClient, creatures);
        TargetMakeEntitiesTargetable(player.connectionToClient, targets, true);
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn, List<CreatureEntity> creatures)
    {
        foreach (var c in creatures) c.CheckIfCanAct();
    }

    [Server]
    public void PlayerFinishedChoosingAttackers(PlayerManager player)
    {
        // From skip or pressing combat button
        _boardManager.DisableReadyButton(player);
        _boardManager.AttackersDeclared(player);

        // Opponent attackable targets stop to be targetable
        var targets = entityZones.GetTechnologies(!player.isLocalPlayer);
        TargetMakeEntitiesTargetable(player.connectionToClient, targets, false);
    }

    #endregion

    #region Blockers

    [Server]
    public void StartDeclareBlockers(List<PlayerManager> players)
    {
        foreach (var player in players)
        {
            // Inverting isLocalPlayer because we want opponent creatures
            var opponentCreatures = entityZones.GetCreatures(!player.isLocalPlayer);
            var playerCreatures = entityZones.GetCreatures(player.isLocalPlayer);
            TargetDeclareBlockers(player.connectionToClient, playerCreatures, opponentCreatures);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<CreatureEntity> creatures, List<CreatureEntity> opponentCreatures)
    {
        // At least one attacking opponent entity
        var isAttacked = opponentCreatures.Exists(entity => entity.IsAttacking);
        
        // At least one creature able to block
        var nbAttackers = 0;
        foreach (var c in creatures) if (c.IsAttacking) nbAttackers++;
        var hasBlocker = creatures.Count != nbAttackers;

        // If both is true -> player may declare blockers (wait until ready button press)
        if (isAttacked && hasBlocker)
        {
            foreach (var creature in creatures) creature.CheckIfCanAct();
            foreach (var c in opponentCreatures) if(c.IsAttacking) c.IsTargetable = true;
        } else {
            print($"Skipping blockers");
            PlayerSkipsBlockers();
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
        var creatures = entityZones.GetAllCreatures();
        foreach (var c in creatures) c.RpcResetAfterCombat();

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

    [TargetRpc]
    private void TargetMakeEntitiesTargetable(NetworkConnection conn, List<BattleZoneEntity> entities, bool targetable){
        foreach(var e in entities) e.IsTargetable = targetable;
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
