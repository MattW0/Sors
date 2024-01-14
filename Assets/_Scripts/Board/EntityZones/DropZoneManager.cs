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
    // public static event Action OnTargetPlayer;
    // public static event Action OnTargetOpponent;
    // public static event Action OnTargetCreatures;
    public static event Action OnResetEntityUI;
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
        if (entity.cardType == CardType.Technology)
        {
            var technology = entity.GetComponent<TechnologyEntity>();
            entityZones.RemoveTechnology(technology, entity.Owner.isLocalPlayer);
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.RemoveCreature(creature, entity.Owner.isLocalPlayer);
        }
    }

    #endregion

    [Server]
    public void EntitiesAreTargetable(PlayerManager owner)
    {
        // TODO: Expand for different possible effect targets and standard combat targeting
        var entities = entityZones.GetAllEntities();
        print("targets count: " + entities.Count);

        TargetMakeEntitiesTargetable(owner.connectionToClient, true, true);
        // TargetMakeEntitiesTargetable(owner.connectionToClient, entities, true);

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
        // TODO: How to get around this?
        var targets = entityZones.GetTechnologies(!player.isLocalPlayer);
        // if(opponent) targets.Add(opponent);

        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        TargetDeclareAttackers(player.connectionToClient, creatures);
        
        // TargetMakeEntitiesTargetable(player.connectionToClient, targets, true);
        TargetMakeEntitiesTargetable(player.connectionToClient, true, true);
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

        // Player creatures stop being able to attack
        var creatures = entityZones.GetCreatures(player.isLocalPlayer);
        TargetMakeCreaturesIdle(player.connectionToClient, creatures);

        // Opponent attackable targets stop to be targetable
        // var targets = entityZones.GetTechnologies(!player.isLocalPlayer);
        // TargetMakeEntitiesTargetable(player.connectionToClient, targets, false);
        TargetMakeEntitiesTargetable(player.connectionToClient, true, false);
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

        // Player creatures stop being able to block
        var creatures = entityZones.GetCreatures(player.isLocalPlayer);
        TargetMakeCreaturesIdle(player.connectionToClient, creatures);
    }
    #endregion

    [Server]
    public void CombatCleanUp()
    {
        RpcEntityUIReset();
        RpcDestroyArrows();
    }

    // TODO: Check if it makes sense to loose health after triggering or at the end of the turn
    // How to handle technologies without trigger ? 
    [Server]
    public void TechnologiesLooseHealth()
    {
        var technologies = entityZones.GetAllTechnologies();

        foreach (var technology in technologies)
        {
            technology.Health -= 1;
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
    private void TargetMakeCreaturesIdle(NetworkConnection conn, List<CreatureEntity> creatures)
    {
        foreach (var c in creatures) c.CanAct = false;
    }

    [TargetRpc]
    private void TargetMakeEntitiesTargetable(NetworkConnection conn, bool combat, bool targetable)
    // private void TargetMakeEntitiesTargetable(NetworkConnection conn, List<BattleZoneEntity> entities, bool targetable)
    {
        // TODO: Logic to only make some entities targetable
        // Tie this to ability target ? 

        // OnStartCombat.Invoke();
        // foreach(var e in entities) e.IsTargetable = targetable;
    }

    [Server]
    public void DestroyTargetArrows()
    {
        var entities = entityZones.GetAllEntities();
        foreach (var entity in entities) entity.RpcResetAfterTarget();

        RpcDestroyArrows();
    }

    [ClientRpc]
    private void RpcEntityUIReset() => OnResetEntityUI?.Invoke();
    [ClientRpc]
    private void RpcDestroyArrows() => OnDestroyArrows?.Invoke();
    [ClientRpc]
    public void RpcHighlightCardHolders(TurnState state) => entityZones.HighlightCardHolders(state);
    [ClientRpc]
    public void RpcResetHolders() => entityZones.ResetHolders();

    internal (List<CreatureEntity> creatures, List<TechnologyEntity> technologies) GetPlayerEntities(PlayerManager player)
    {
        var creatures = entityZones.GetCreatures(player.isLocalPlayer);
        var technologies = entityZones.GetTechnologies(player.isLocalPlayer);

        return (creatures, technologies);
    }

    #endregion
}
