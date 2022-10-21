using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private List<MoneyZone> moneyZones;
    [SerializeField] private List<EntityManager> entityManagers;

    private Dictionary<PlayerManager, List<BattleZoneEntity>> _battleZoneEntities;
    [SerializeField] private List<BattleZoneEntity> attackers;

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
        entityManagers[0].RpcHighlight(active); 
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

    private void DeclareBlockers() {
        
        foreach (var entity in attackers)
        {
            entity.RpcHighlightAttacker();
        }
        
        print("Declare blockers!");
        foreach (var entityManager in entityManagers)
        {
            if (entityManager.GetEntities().Count == 0)
            {
                OnSkipCombatPhase?.Invoke();
                return;
            }
            entityManager.RpcDeclareBlockers();
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
