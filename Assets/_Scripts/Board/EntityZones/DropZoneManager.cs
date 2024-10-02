using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DropZoneManager : NetworkBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private EntityZones entityZones;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;    
    public static event Action OnCombatStart;
    public static event Action<bool> OnDeclareAttackers;
    public static event Action<bool> OnDeclareBlockers;
    public static event Action OnCombatEnd;
    public static event Action<EffectTarget> OnTargetEntities;
    public static event Action OnResetEntityUI;
    public static event Action OnDestroyArrows;

    #region Entities ETB and LTB

    [Server]
    public async UniTaskVoid EntitiesEnterDropZone(Dictionary<GameObject, BattleZoneEntity> entities)
    {   
        foreach (var (card, entity) in entities){
            // Need to await RPC for initialization
            while (!entity.Owner) await UniTask.Delay(100);
            var owner = entity.Owner;

            // To show which card spawns an entity 
            owner.RpcMoveCard(card, CardLocation.Hand, CardLocation.EntitySpawn);
            entityZones.RpcMoveEntityToSpawned(entity);

            // Keep track of entity objects for combat interactions
            entityZones.AddEntity(entity, owner.isLocalPlayer);

            // Spawning animation
            await UniTask.Delay(SorsTimings.showSpawnedEntity);
            entityZones.RpcMoveEntityToHolder(entity);
            owner.RpcMoveCard(card, CardLocation.EntitySpawn, CardLocation.PlayZone);

            // Score points on ETB if card is a Technology
            if (entity.CardInfo.type == CardType.Technology) 
                entity.Owner.Score += entity.GetComponent<TechnologyEntity>().Points;
        }
    }

    [Server]
    public void EntityLeavesPlayZone(BattleZoneEntity entity)
    {
        if (entity.cardType == CardType.Technology)
        {
            var technology = entity.GetComponent<TechnologyEntity>();
            entityZones.RemoveTechnology(technology, entity.Owner.isLocalPlayer);

            technology.Owner.Score -= technology.Points;
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            entityZones.RemoveCreature(creature, entity.Owner.isLocalPlayer);
        }
    }

    #endregion

    [TargetRpc]
    public void TargetEntitiesAreTargetable(NetworkConnection conn, EffectTarget target)
    {
        // TODO: Expand for different possible effect targets and standard combat targeting

        var e = entityZones.GetAllEntities();
        var nbTargets = e.Count;
        print("targets count: " + nbTargets);
        // if (nbTargets == 0)
        OnTargetEntities?.Invoke(target);
    }

    [ClientRpc]
    public void RpcResetTargeting() => OnTargetEntities?.Invoke(EffectTarget.None);

    #region Attackers

    [Server]
    public void StartDeclareAttackers(List<PlayerManager> players)
    {
        foreach (var player in players)
        {
            // Auto-skip : Local player has no creatures
            if (entityZones.GetCreatures(player.isLocalPlayer).Count == 0) PlayerFinishedChoosingAttackers(player);
            // else if (player.isAI) PlayerFinishedChoosingAttackers(player);
            else TargetDeclareAttackers(player.connectionToClient);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn)
    {
        OnDeclareAttackers?.Invoke(true);
        OnCombatStart?.Invoke();
    }

    [Server]
    public void PlayerFinishedChoosingAttackers(PlayerManager player)
    {
        // From skip or pressing combat button
        _boardManager.AttackersDeclared(player);
        TargetFinishChoosingAttackers(player.connectionToClient);
    }

    [TargetRpc]
    private void TargetFinishChoosingAttackers(NetworkConnection conn) => OnDeclareAttackers?.Invoke(false);

    #endregion

    #region Blockers

    [Server]
    public void StartDeclareBlockers(List<PlayerManager> players)
    {
        foreach (var player in players)
        {
            // Inverting isLocalPlayer because we want opponent creatures
            var opponentCreatures = entityZones.GetCreatures(!player.isLocalPlayer);

            // Auto-skip : No attacking opponent creature
            var isAttacked = opponentCreatures.Exists(entity => entity.IsAttacking);
            if(!isAttacked) {
                PlayerFinishedChoosingBlockers(player);
                continue;
            }
            
            // Auto-skip : No creature able to block
            var playerCreatures = entityZones.GetCreatures(player.isLocalPlayer);
            var hasBlocker = playerCreatures.Exists(entity => !entity.IsAttacking);
            if(!hasBlocker) {
                PlayerFinishedChoosingBlockers(player);
                continue;
            }
            
            TargetDeclareBlockers(player.connectionToClient, playerCreatures, opponentCreatures);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<CreatureEntity> playerCreatures, List<CreatureEntity> opponentCreatures)
    {
        OnDeclareBlockers?.Invoke(true);
        OnCombatEnd?.Invoke();
    }

    [Server]
    public void PlayerFinishedChoosingBlockers(PlayerManager player)
    {
        _boardManager.BlockersDeclared(player);
        TargetFinishChoosingBlockers(player.connectionToClient);
    }

    [TargetRpc]
    private void TargetFinishChoosingBlockers(NetworkConnection conn) => OnDeclareBlockers?.Invoke(false);
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
