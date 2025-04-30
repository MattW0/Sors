using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using SorsGameState;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(TriggerHandler))]
public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }
    private GameManager _gameManager;
    private CombatManager _combatManager;
    private InteractionPanel _interactionPanel;
    [SerializeField] private DropZoneManager _dropZone;
    private List<BattleZoneEntity> _deadEntities = new();
    private TurnState _combatState;
    private GameState _gameState;

    private void Awake()
    {
        if (!Instance) Instance = this;

        CombatManager.OnCombatStateChanged += StartCombatState;
        BattleZoneEntity.OnEntityDies += EntityDies;
        EffectHandler.OnPlayerStartSelectTarget += PlayerStartSelectTarget;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _combatManager = CombatManager.Instance;
        _interactionPanel = InteractionPanel.Instance;
    }

    // Move entities to holders and card into played zone
    // Async for game state loader
    public async UniTask PlayEntities(Dictionary<GameObject, BattleZoneEntity> entities) => await _dropZone.EntitiesEnter(entities);

    #region Effects

    public bool PlayerHasValidTarget(Ability ability)
    {
        var numberTargetables = _dropZone.GetNumberTargets(ability.target);
        // print("Ability " + ability.ToString() + " has " + numberTargetables + " targets");
        return numberTargetables > 0;
    }

    private void PlayerStartSelectTarget(BattleZoneEntity entity, Ability ability)
    {
        var owner = entity.Owner;

        // Target rpc let triggering player choose
        owner.TargetPlayerStartChooseTarget();
        entity.TargetSpawnTargetArrow(owner.connectionToClient);

        _dropZone.TargetEntitiesAreTargetable(owner.connectionToClient, ability.target);
    }

    public void ResetTargeting() => _dropZone.RpcResetTargeting();

    #endregion

    #region Combat

    private void StartCombatState(TurnState state)
    {
        _combatState = state;
        CombatStateTransition().Forget();
    }
    private async UniTaskVoid CombatStateTransition()
    {
        await UniTask.Delay(SorsTimings.wait);
        
        if (_combatState == TurnState.Attackers) StartAttackers();
        else if (_combatState == TurnState.Blockers) StartBlockers();
        else if (_combatState == TurnState.CombatDamage) StartDamage();
        else if (_combatState == TurnState.CombatCleanUp) CombatCleanUp().Forget();
    }

    private void StartAttackers()
    {
        foreach (var player in _gameManager.players.Values)
        {
            var canAttack = _dropZone.HasAttacker(player);
            _interactionPanel.TargetStartCombatState(player.connectionToClient, _combatState, !canAttack);

            if (canAttack) _dropZone.TargetDeclareAttackers(player.connectionToClient);
            else PlayerConfirmsCombatState(player);
        }
    }

    public void PlayerChoosesTargetToAttack(BattleZoneEntity target, List<CreatureEntity> attackers)
    {
        _combatManager.PlayerChoosesTargetToAttack(target, attackers);
    }

    private void StartBlockers()
    {
        foreach (var player in _gameManager.players.Values)
        {
            var canBlock = _dropZone.HasBlocker(player);
            _interactionPanel.TargetStartCombatState(player.connectionToClient, _combatState, !canBlock);
            
            if (canBlock) _dropZone.TargetDeclareBlockers(player.connectionToClient);
            else PlayerConfirmsCombatState(player);
        }
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker, List<CreatureEntity> blockers)
    {
        _combatManager.PlayerChoosesAttackerToBlock(attacker, blockers);
    }

    public void PlayerConfirmsCombatState(PlayerManager player)
    {
        if (_combatState == TurnState.Attackers) 
        {
            _dropZone.TargetFinishChoosingAttackers(player.connectionToClient);
            _combatManager.PlayerDeclaredAttackers(player);
        }
        else if (_combatState == TurnState.Blockers) 
        {
            _dropZone.TargetFinishChoosingBlockers(player.connectionToClient);
            _combatManager.PlayerDeclaredBlockers(player);
        }
    }

    private void StartDamage()
    {
        foreach (var player in _gameManager.players.Values)
        {
            _interactionPanel.TargetStartCombatState(player.connectionToClient, _combatState, false);
        }
    }

    public void EntityDies(BattleZoneEntity entity)
    {
        // Catch exception where entity was already dead and received more damage
        if (_deadEntities.Contains(entity)) return;

        // print($"{entity.Title} dies");
        _deadEntities.Add(entity);

        // Somehow NetworkServer.Destroy(this) destroys the GO but does not call OnDestroy(),
        // Thus, do it here manually to prevent null references when events are triggered
        entity.RpcUnsubscribeEvents();
    }

    private async UniTask CombatCleanUp()
    {
        await ClearDeadEntities();
        _dropZone.CombatCleanUp();
    }
    #endregion

    public async UniTask BoardCleanUp()
    {
        await ClearDeadEntities();
        _dropZone.DestroyTargetArrows();
    }

    public async UniTask BoardCleanUpEndOfTurn(List<CardInfo>[] scriptableTiles)
    {
        await ClearDeadEntities();

        // _dropZone.TechnologiesLooseHealth();
        if(isServer) SaveGameState(scriptableTiles);
    }

    private async UniTask ClearDeadEntities()
    {
        print($"    - BoardManager: Clearing {_deadEntities.Count} dead entities");
        await _dropZone.EntitiesLeave(_deadEntities);

        foreach (var dead in _deadEntities)
        {
            dead.UnsubscribeEvents();
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();

        await UniTask.Delay(SorsTimings.waitShort);
    }

    #region Game State

    public void PrepareGameStateFile(List<CardInfo>[] scriptableTiles)
    {
        _gameState = new GameState(_gameManager.players.Count);

        int i = 0;
        foreach (var player in _gameManager.players.Values){
            _gameState.players[i] = new Player(player.PlayerName, player.isLocalPlayer);
            i++;
        }

        SaveGameState(scriptableTiles);
    }

    private void SaveGameState(List<CardInfo>[] scriptableTiles)
    {
        _gameState.market.SaveMarketState(scriptableTiles);

        int i = 0;
        foreach(var player in _gameManager.players.Values){
            _gameState.players[i].SavePlayerState(player);
            
            var (creatures, technologies) = _dropZone.GetPlayerEntities(player);

            _gameState.players[i].entities.creatures.Clear();
            _gameState.players[i].entities.technologies.Clear();

            foreach(var entity in creatures){
                var e = new Entity(entity.CardInfo, entity.Health, entity.Attack);
                _gameState.players[i].entities.creatures.Add(e);
            }
            foreach(var entity in technologies){
                var e = new Entity(entity.CardInfo, entity.Health);
                _gameState.players[i].entities.technologies.Add(e);
            }

            i++;
        }
        
        _gameState.SaveState(_gameManager.turnNumber);
    }
    #endregion

    #region UI

    public int CheckNumberOfFreeSlots(bool isHost, TurnState state) => _dropZone.GetNumberOfFreeSlots(isHost, state);
    public void ResetHolders() => _dropZone.RpcResetHolders();
    // public void DiscardMoney() => _dropZone.RpcDiscardMoney();
    #endregion
    private void OnDestroy()
    {
        // GameManager.OnGameStart -= PrepareGameStateFile;
        CombatManager.OnCombatStateChanged -= StartCombatState;
        BattleZoneEntity.OnEntityDies -= EntityDies;
        EffectHandler.OnPlayerStartSelectTarget -= PlayerStartSelectTarget;
    }

}
