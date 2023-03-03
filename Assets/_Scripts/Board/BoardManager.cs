using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }
    private GameManager _gameManager;
    [SerializeField] private DropZoneManager dropZone;
    private PlayerInterfaceManager _playerInterfaceManager;

    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsCache = new();
    private List<BattleZoneEntity> _deadEntities = new();
    public List<BattleZoneEntity> boardAttackers { get; private set; }

    public static event Action<PlayerManager, BattleZoneEntity> OnEntityAdded;
    // public static event Action<PlayerManager> OnSkipCombatPhase;
    public static event Action<PlayerManager> OnAttackersDeclared;
    public static event Action<PlayerManager> OnBlockersDeclared;

    private void Awake() {
        if (!Instance) Instance = this;

        _gameManager = GameManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
        boardAttackers = new List<BattleZoneEntity>();

        GameManager.OnEntitySpawned += AddEntity;
        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += DeclareBlockers;

    }

    private void AddEntity(PlayerManager owner, BattleZoneEntity entity, GameObject card) {
        // To keep track which card object corresponds to which entity
        _entitiesObjectsCache.Add(entity, card);

        entity.OnDeath += EntityDies;
        OnEntityAdded?.Invoke(owner, entity);
    }

    #region Combat

    private void DeclareAttackers() => dropZone.PlayersDeclareAttackers(_gameManager.players.Keys.ToList());

    public void AttackersDeclared(PlayerManager player, List<BattleZoneEntity> playerAttackers) {

        _playerInterfaceManager.RpcLog(player.PlayerName + " declared " + playerAttackers.Count + " attackers.");
        // Is 0 for auto-skip or no attackers declared
        if (playerAttackers.Count > 0) {
            foreach (var a in playerAttackers) boardAttackers.Add(a);
        }

        OnAttackersDeclared?.Invoke(player);
    }

    public void ShowOpponentAttackers() {
        // Share attacker state accross clients
        foreach (var entity in boardAttackers){
            entity.IsAttacking = true;
            entity.RpcIsAttacker();
        }
    }

    private void DeclareBlockers() => dropZone.PlayersDeclareBlockers(_gameManager.players.Keys.ToList());

    public void BlockersDeclared(PlayerManager player, List<BattleZoneEntity> playerBlockers) {
        
        _playerInterfaceManager.RpcLog(player.PlayerName + " declared " + playerBlockers.Count + " blockers.");

        OnBlockersDeclared?.Invoke(player);
    }
    
    private void EntityDies(PlayerManager owner, BattleZoneEntity entity)
    {
        entity.OnDeath -= EntityDies;

        // Update lists of active entities 
        _deadEntities.Add(entity);
    }

    public void CombatCleanUp()
    {
        DestroyArrows();
        foreach (var dead in _deadEntities)
        {
            _playerInterfaceManager.RpcLog(dead.Title + " dies for real now");
            boardAttackers.Remove(dead);
            
            // Move the card object to discard pile
            dead.Owner.discard.Add(_entitiesObjectsCache[dead].GetComponent<CardStats>().cardInfo);
            dead.Owner.RpcMoveCard(_entitiesObjectsCache[dead],
                CardLocations.PlayZone, CardLocations.Discard);

            _entitiesObjectsCache.Remove(dead);
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();

        foreach (var attacker in boardAttackers){
            attacker.RpcRetreatAttacker();
        }
        boardAttackers.Clear();

        dropZone.CombatCleanUp();
    }

    // public void PlayerSkipsCombatPhase(PlayerManager player) => OnSkipCombatPhase?.Invoke(player);

    #endregion

    #region UI
    public void ShowCardPositionOptions(bool active)
    {
        // Resetting with active=false at end of Deploy phase
        // entityManagers[0] = PlayerDropZone (not opponents)
        dropZone.RpcHighlightCardHolders(active); 
    }

    public void DisableReadyButton(PlayerManager player){
        _playerInterfaceManager.TargetDisableReadyButton(player.connectionToClient);
    }

    public void ResetHolders()
    {
        dropZone.RpcResetHolders();
    }

    private void DestroyArrows()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            player.RpcDestroyArrows();
        }
    }

    public void DiscardMoney() => dropZone.RpcDiscardMoney();
    #endregion
    
    private void OnDestroy()
    {
        GameManager.OnEntitySpawned -= AddEntity;
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
