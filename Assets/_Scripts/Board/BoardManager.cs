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
    private CombatManager _combatManager;
    private GameManager _gameManager;
    private TurnManager _turnManager;
    // public List<BattleZoneEntity> Attackers { get; private set; }

    private void Awake()
    {
        if (!Instance) Instance = this;

        TurnManager.OnTursStarting += Prepare;
        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += RpcDeclareBlockers;
    }

    [Server]
    private void Prepare() {
        
        _combatManager = CombatManager.Instance;
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        
        _battleZoneEntities = new Dictionary<PlayerManager, List<BattleZoneEntity>>();
        foreach (var player in _gameManager.players.Keys)
        {
            print("Adding Entity list to player");
            _battleZoneEntities[player] = new List<BattleZoneEntity>();
        }
    }

    [Server]
    public void AddEntity(PlayerManager player, BattleZoneEntity entity)
    {
        _battleZoneEntities[player].Add(entity);
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

    [ClientRpc]
    private void DeclareAttackers() {
        
        print(_battleZoneEntities.Count + " cards on field");

        foreach (var entitiesList in _battleZoneEntities.Values)
        {
            if (entitiesList.Count == 0)
            {
                print("No entity on battlefield");
                //     combatManager.CmdPlayerSkipsPhase();
                continue;
            }
            
            foreach (var entity in entitiesList)
            {
                entity.CanAct(CombatState.Attackers);
            }
        }
    }
    
    private void AttackerDeclared(BattleZoneEntity attacker, bool adding)
    {
        print("Adding attacker "+ attacker.Title + ": " + adding);
        
        // if (!adding)
        // {
        //     Attackers.Remove(attacker);
        //     return;
        // }
        //
        // Attackers.Add(attacker);
    }

    [ClientRpc]
    private void RpcDeclareBlockers() {
        if (!hasAuthority) return;
        print("Declare blockers!");
        
    }
    
    private void OnDestroy()
    {
        TurnManager.OnTursStarting -= Prepare;
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= RpcDeclareBlockers;
    }
}
