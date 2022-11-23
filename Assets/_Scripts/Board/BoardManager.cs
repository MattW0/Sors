using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private List<MoneyZone> moneyZones;
    [SerializeField] private List<EntityManager> entityManagers;

    // Owner, entities
    private Dictionary<PlayerManager, List<BattleZoneEntity>> _battleZoneEntities;
    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsDict = new();
    private List<BattleZoneEntity> _deadEntities = new();
    public List<BattleZoneEntity> attackers { get; private set; }
    
    private GameManager _gameManager;

    public static event Action<BattleZoneEntity> OnEntityAdded;
    public static event Action<PlayerManager> OnSkipCombatPhase;
    // public static event Action OnDestroyArrows;

    private void Awake()
    {
        if (!Instance) Instance = this;

        TurnManager.OnTurnsStarting += Prepare;
        GameManager.OnEntitySpawned += AddEntity;
        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += DeclareBlockers;
    }

    private void Prepare() {
        
        _gameManager = GameManager.Instance;
        
        _battleZoneEntities = new Dictionary<PlayerManager, List<BattleZoneEntity>>();
        attackers = new List<BattleZoneEntity>();

        foreach (var player in _gameManager.players.Keys)
        {
            _battleZoneEntities[player] = new List<BattleZoneEntity>();
        }
    }

    private void AddEntity(PlayerManager player, BattleZoneEntity entity, GameObject card)
    {
        _battleZoneEntities[player].Add(entity);
        _entitiesObjectsDict.Add(entity, card);

        entity.OnDeath += EntityDies;
        OnEntityAdded?.Invoke(entity);
    }
    
    public void DiscardMoney()
    {
        foreach (var zone in entityManagers)
        {
            zone.RpcDiscardMoney();
        }
    }
    
    public void ShowCardPositionOptions(bool active)
    {
        // Resetting with active=false at end of Deploy phase
        // entityManagers[0] = PlayerDropZone (not opponents)
        entityManagers[0].RpcHighlightCardHolders(active); 
    }

    public void ResetHolders()
    {
        entityManagers[0].RpcResetHolders();
    }

    private void DeclareAttackers() {
        
        foreach (var entityManager in entityManagers)
        {
            entityManager.RpcDeclareAttackers();
        }
        
        // Auto-skipping if player has empty board
        foreach (var item in _battleZoneEntities)
        {
            if (item.Value.Count > 0) continue;
            
            OnSkipCombatPhase?.Invoke(item.Key);
            print("No attackers available");
        }
    }
    
    public void AttackerDeclared(BattleZoneEntity attacker, bool adding)
    {
        if (adding) attackers.Add(attacker);
        else attackers.Remove(attacker);
    }

    public void PlayerFinishedChoosingAttackers(PlayerManager player)
    {
        foreach (var entity in _battleZoneEntities[player])
        {
            if (entity.IsAttacking) entity.TargetIsAttacker(player.connectionToClient);
            else entity.TargetCanNotAct(player.connectionToClient);
        }
    }
    
    public void PlayerFinishedChoosingBlockers(PlayerManager player)
    {
        foreach (var entity in _battleZoneEntities[player])
        {
            if (entity.IsAttacking) continue;
            entity.TargetCanNotAct(player.connectionToClient);
        }
    }

    private void DeclareBlockers() {
        
        foreach (var entity in attackers)
        {
            entity.RpcIsAttacker();
        }
        
        foreach (var entityManager in entityManagers)
        {
            entityManager.RpcDeclareBlockers();
        }

        // Check for auto-skip
        foreach (var item in _battleZoneEntities)
        {
            var list = item.Value;
            // Skip if either player has empty board or has declared all attackers
            var hasBlocker = false;
            foreach (var entity in list.Where(entity => !entity.IsAttacking))
            {
                hasBlocker = true;
            }
            if (hasBlocker) continue;
            
            print("No blockers available");
            OnSkipCombatPhase?.Invoke(item.Key);
        }
    }
    
    private void EntityDies(BattleZoneEntity entity)
    {
        print("Entity " + entity.Title + " dies.");
        entity.OnDeath -= EntityDies;

        // Update lists of active entities 
        _deadEntities.Add(entity);
        _battleZoneEntities[entity.Owner].Remove(entity);
    }

    public void CombatCleanUp()
    {
        DestroyArrows();
        
        attackers.Clear();
        foreach (var dead in _deadEntities)
        {
            print(dead.Title + " dies");
            NetworkServer.Destroy(dead.gameObject);
            
            // Move the card object to discard pile
            dead.Owner.RpcMoveCard(_entitiesObjectsDict[dead],
                CardLocations.PlayZone, CardLocations.Discard);
            _entitiesObjectsDict.Remove(dead);
        }
        _deadEntities.Clear();
        
        foreach (var list in _battleZoneEntities.Values)
        {
            foreach (var entity in list)
            {
                entity.RpcResetAfterCombat();
            }
        }
    }

    private void DestroyArrows()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            player.RpcDestroyArrows();
        }
    }
    
    private void OnDestroy()
    {
        GameManager.OnEntitySpawned -= AddEntity;
        TurnManager.OnTurnsStarting -= Prepare;
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
