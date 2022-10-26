using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private List<MoneyZone> moneyZones;
    [SerializeField] private List<EntityManager> entityManagers;

    // Owner, entities
    private Dictionary<PlayerManager, List<BattleZoneEntity>> _battleZoneEntities;
    // Attacking entity, target player
    public List<BattleZoneEntity> attackers { get; private set; }

    private CombatManager _combatManager;
    private GameManager _gameManager;
    private TurnManager _turnManager;
    // public List<BattleZoneEntity> Attackers { get; private set; }

    public static event Action<BattleZoneEntity> OnEntityAdded;
    public static event Action OnSkipCombatPhase; 

    private void Awake()
    {
        if (!Instance) Instance = this;

        GameManager.OnEntitySpawned += AddEntity;
        TurnManager.OnTursStarting += Prepare;
        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += DeclareBlockers;
    }

    private void Prepare() {
        
        _combatManager = CombatManager.Instance;
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        
        _battleZoneEntities = new Dictionary<PlayerManager, List<BattleZoneEntity>>();
        attackers = new List<BattleZoneEntity>();

        foreach (var player in _gameManager.players.Keys)
        {
            print("Adding Entity list to player");
            _battleZoneEntities[player] = new List<BattleZoneEntity>();
        }
    }

    private void AddEntity(PlayerManager player, BattleZoneEntity entity)
    {
        _battleZoneEntities[player].Add(entity);
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
        entityManagers[0].RpcHighlightEntities(active); 
    }

    private void DeclareAttackers() {
        
        foreach (var entityManager in entityManagers)
        {
            entityManager.RpcDeclareAttackers();
        }
        
        // Auto-skipping if player has empty board
        foreach (var list in _battleZoneEntities.Values)
        {
            if (list.Count > 0) continue;
            
            OnSkipCombatPhase?.Invoke();
            print("Skipping due to empty board");
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
            entity.TargetIsAttacker(player.connectionToClient);
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

        // Auto-skipp
        foreach (var list in _battleZoneEntities.Values)
        {
            // Skip if either player has empty board or has declared all attackers
            var allAreAttacking = list.Count == attackers.Count;
            if (list.Count > 0 || !allAreAttacking) continue; 
            
            print("No blockers available");
            for (var i=0; i<_gameManager.players.Count; i++) OnSkipCombatPhase?.Invoke();
        }
    }
    
    private void OnDestroy()
    {
        GameManager.OnEntitySpawned -= AddEntity;
        TurnManager.OnTursStarting -= Prepare;
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
