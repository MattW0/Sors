using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }
    private GameManager _gameManager;
    [SerializeField] private PlayerZoneManager playerZone;
    [SerializeField] private PlayerZoneManager opponentZone;

    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsDict = new();
    private List<BattleZoneEntity> _deadEntities = new();
    public List<BattleZoneEntity> attackers { get; private set; }

    public static event Action<BattleZoneEntity> OnEntityAdded;
    public static event Action<PlayerManager> OnSkipCombatPhase;

    private void Awake()
    {
        if (!Instance) Instance = this;

        GameManager.OnEntitySpawned += AddEntity;
        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += DeclareBlockers;

        _gameManager = GameManager.Instance;
        attackers = new List<BattleZoneEntity>();
    }

    private void AddEntity(BattleZoneEntity entity, GameObject card)
    {
        // To keep track which card object corresponds to which entity
        _entitiesObjectsDict.Add(entity, card);

        entity.OnDeath += EntityDies;
        OnEntityAdded?.Invoke(entity);
    }

    #region Combat

    private void DeclareAttackers() => playerZone.RpcDeclareAttackers();
    
    public void AttackerDeclared(BattleZoneEntity attacker, bool adding)
    {
        if (adding) attackers.Add(attacker);
        else attackers.Remove(attacker);
    }

    public void PlayerFinishedChoosingAttackers(PlayerManager player)
    {
        playerZone.TargetPlayerFinishedChoosingAttackers(player.connectionToClient);
    }

    public void ShowOpponentAttackers(){
        // Share attacker state accross clients
        foreach (var entity in attackers)
        {
            entity.RpcIsAttacker();
        }
    }

    private void DeclareBlockers() => playerZone.RpcDeclareBlockers(opponentZone.GetAttackersCount());
    
    private void EntityDies(BattleZoneEntity entity)
    {
        print("Entity " + entity.Title + " dies.");
        entity.OnDeath -= EntityDies;

        // Update lists of active entities 
        _deadEntities.Add(entity);
    }

    public void CombatCleanUp()
    {
        DestroyArrows();

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

        foreach (var attacker in attackers){
            attacker.RpcRetreatAttacker();
        }
        attackers.Clear();

        playerZone.RpcCombatCleanUp();
    }

    public void PlayerSkipsCombatPhase(PlayerManager player) => OnSkipCombatPhase?.Invoke(player);

    #endregion

    #region UI
    public void ShowCardPositionOptions(bool active)
    {
        // Resetting with active=false at end of Deploy phase
        // entityManagers[0] = PlayerDropZone (not opponents)
        playerZone.RpcHighlightCardHolders(active); 
    }

    public void ResetHolders()
    {
        playerZone.RpcResetHolders();
    }

    private void DestroyArrows()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            player.RpcDestroyArrows();
        }
    }

    public void DiscardMoney()
    {
        playerZone.RpcDiscardMoney();
        opponentZone.RpcDiscardMoney();
    }
    #endregion
    
    private void OnDestroy()
    {
        GameManager.OnEntitySpawned -= AddEntity;
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
