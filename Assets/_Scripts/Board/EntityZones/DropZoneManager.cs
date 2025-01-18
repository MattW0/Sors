using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Rendering;

[RequireComponent(typeof(EntityZones))]
public class DropZoneManager : NetworkBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    [SerializeField] private TriggerHandler _triggerHandler;
    private EntityZones _entityZones;
    // Entities and corresponding card object (for dying)
    [SerializeReference] private EntitiesCardsDictionary _entitiesCardsCache = new();
    public static event Action<bool> OnDeclareAttackers;
    public static event Action<bool> OnDeclareBlockers;
    public static event Action OnCombatEnd;
    public static event Action<Target> OnTargetEntities;
    public static event Action OnResetEntityUI;
    public static event Action OnDestroyArrows;

    #region Entities ETB and LTB

    private void Awake()
    {
        _entityZones = GetComponent<EntityZones>();
    }

    [Server]
    public async UniTask EntitiesEnter(Dictionary<GameObject, BattleZoneEntity> entities)
    {
        foreach (var (card, entity) in entities)
        {
            // Initialize and keep track which card object corresponds to which entity
            _entitiesCardsCache.Add(entity, card);

            // Need to await RPC for initialization
            while (!entity.Owner) await UniTask.Delay(10);
            var owner = entity.Owner;

            // Track entity and evaluate where it will be placed
            _entityZones.RpcAddEntity(entity, entity.Owner.isLocalPlayer);

            // To show which card spawns an entity -> move to spawn
            owner.Cards.RpcMoveCard(card, CardLocation.Hand, CardLocation.EntitySpawn);
            _entityZones.RpcMoveEntityToSpawned(entity);
            await UniTask.Delay(SorsTimings.showSpawnedEntity);

            // _entityZones.MoveToHolder(entity);
            entity.RpcMoveToHolder();
            owner.Cards.RpcMoveCard(card, CardLocation.EntitySpawn, CardLocation.PlayZone);
            await UniTask.Delay(SorsTimings.moveSpawnedCard);

            // Score points on ETB if card is a Technology
            if (entity.CardInfo.type == CardType.Technology) 
                entity.Owner.Score += entity.GetComponent<TechnologyEntity>().Points;
            
            // Check for ETB and if phase start trigger gets added to phases being tracked
            _triggerHandler.EntityEnters(entity);
        }
    }

    [Server]
    public async UniTask EntitiesLeave(List<BattleZoneEntity> entities)
    {
        foreach(var dead in entities)
        {
            print("Entity dies: " + dead.CardInfo.title + " Owner: " + dead.Owner.PlayerName);
            // Remove triggers 
            _triggerHandler.EntityLeaves(dead);
            _entityZones.RpcRemoveEntity(dead, dead.Owner.isLocalPlayer);

            // Get corresponding card object and remove from cache
            var cardObject = _entitiesCardsCache[dead];
            _entitiesCardsCache.Remove(dead);

            // Substract points on LTB if card is a Technology
            if (dead.CardInfo.type == CardType.Technology) 
                dead.Owner.Score -= dead.GetComponent<TechnologyEntity>().Points;

            // TODO: Dying animation here
            await UniTask.Delay(10);

            // Move the card object to discard pile
            dead.Owner.Cards.discard.Add(cardObject.GetComponent<CardStats>());
            dead.Owner.Cards.RpcMoveCard(cardObject, CardLocation.PlayZone, CardLocation.Discard);
            await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.cardMoveTime));
        }
    }

    #endregion

    [Server]
    public int GetNumberTargets(Target target)
    {
        // TODO: Expand for different possible effect targets and standard combat targeting

        if (target == Target.None) return 0;
        if (target == Target.Self) return 1;
        if (target == Target.You || target == Target.Opponent) return 1;
        if (target == Target.AnyPlayer) return 2; // 2 Players
        if (target == Target.Creature) return _entityZones.GetAllCreatures().Count;
        if (target == Target.Technology) return _entityZones.GetAllTechnologies().Count;
        if (target == Target.Entity) return _entityZones.GetAllEntities().Count;
        if (target == Target.Any) return _entityZones.GetAllEntities().Count + 2; // 2 Players

        return -1;
    }


    [TargetRpc]
    public void TargetEntitiesAreTargetable(NetworkConnection conn, Target target) => OnTargetEntities?.Invoke(target);

    [ClientRpc]
    public void RpcResetTargeting() => OnTargetEntities?.Invoke(Target.None);

    #region Attackers

    [Server]
    public void StartDeclareAttackers(List<PlayerManager> players)
    {
        foreach (var player in players)
        {
            // Auto-skip : Local player has no creatures
            if (_entityZones.GetCreatures(player.isLocalPlayer).Count == 0) 
                PlayerFinishedChoosingAttackers(player);

            // else if (player.isAI) PlayerFinishedChoosingAttackers(player);
            else TargetDeclareAttackers(player.connectionToClient);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn) => OnDeclareAttackers?.Invoke(true);

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
            var opponentCreatures = _entityZones.GetCreatures(!player.isLocalPlayer);

            // Auto-skip : No attacking opponent creature
            var isAttacked = opponentCreatures.Exists(entity => entity.IsAttacking);
            if(!isAttacked) {
                PlayerFinishedChoosingBlockers(player);
                continue;
            }
            
            // Auto-skip : No creature able to block
            var playerCreatures = _entityZones.GetCreatures(player.isLocalPlayer);
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
        var technologies = _entityZones.GetAllTechnologies();

        foreach (var technology in technologies)
        {
            technology.Health -= 1;
        }
    }

    #region UI and utils

    // For GameState saving
    [Server]
    internal (List<CreatureEntity> creatures, List<TechnologyEntity> technologies) GetPlayerEntities(PlayerManager player)
    {
        var creatures = _entityZones.GetCreatures(player.isLocalPlayer);
        var technologies = _entityZones.GetTechnologies(player.isLocalPlayer);

        return (creatures, technologies);
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        playerMoneyZone.DiscardMoney();
        opponentMoneyZone.DiscardMoney();
    }

    [Server]
    public void DestroyTargetArrows() => RpcDestroyArrows();
    [ClientRpc]
    public void RpcResetHolders() => _entityZones.ResetHolders();
    [ClientRpc]
    private void RpcEntityUIReset() => OnResetEntityUI?.Invoke();
    [ClientRpc]
    private void RpcDestroyArrows() => OnDestroyArrows?.Invoke();
    [ClientRpc]
    private void RpcHighlightCardHolders(TurnState state) => _entityZones.HighlightCardHolders(state);

    [Server]
    internal int GetNumberOfFreeSlots(bool isHost, TurnState state)
    {
        var numberSlotsAvailable = _entityZones.GetNumberOfFreeHolders(isHost, state);
        if (numberSlotsAvailable > 0) RpcHighlightCardHolders(state);
        
        return numberSlotsAvailable;
    } 

    #endregion
}
