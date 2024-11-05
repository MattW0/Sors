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
    [SerializeField] private DropZoneManager _dropZone;
    [SerializeField] private PhasePanel _phasePanel;

    private List<BattleZoneEntity> _deadEntities = new();
    private TurnState _combatState;
    private GameState _gameState;

    private void Awake()
    {
        if (!Instance) Instance = this;

        CombatManager.OnCombatStateChanged += StartCombatPhase;
        BattleZoneEntity.OnEntityDies += EntityDies;
        EffectHandler.OnPlayerStartSelectTarget += PlayerStartSelectTarget;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _combatManager = CombatManager.Instance;
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

    public void StartCombatPhase(TurnState state)
    {
        _combatState = state;
        _phasePanel.RpcStartCombatPhase(state);

        CombatTransitionAnimation(state).Forget();
    }

    private void DeclareAttackers() => _dropZone.StartDeclareAttackers(_gameManager.players.Values.ToList());
    public void AttackersDeclared(PlayerManager player)
    {
        _phasePanel.TargetDisableCombatButtons(player.connectionToClient);
        _combatManager.PlayerDeclaredAttackers(player);
    }
    
    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Values.ToList());
    public void BlockersDeclared(PlayerManager player)
    {
        _phasePanel.TargetDisableCombatButtons(player.connectionToClient);
        _combatManager.PlayerDeclaredBlockers(player);
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

    private async void CombatCleanUp()
    {
        await ClearDeadEntities();
        _dropZone.CombatCleanUp();
    }
    #endregion

    public async void BoardCleanUp()
    {
        await ClearDeadEntities();
        _dropZone.DestroyTargetArrows();
    }

    public async void BoardCleanUpEndOfTurn(List<CardInfo>[] scriptableTiles)
    {
        BoardCleanUp();
        await ClearDeadEntities();

        // _dropZone.TechnologiesLooseHealth();
        if(isServer) SaveGameState(scriptableTiles);
    }

    private async UniTask ClearDeadEntities()
    {
        await _dropZone.EntitiesLeave(_deadEntities);
        print("Clearing dead entities");

        foreach (var dead in _deadEntities)
        {
            dead.UnsubscribeEvents();
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();
    }

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

    #region UI
    public void PlayerPressedReadyButton(PlayerManager player){
        if (_combatState == TurnState.Attackers)
        {
            _dropZone.PlayerFinishedChoosingAttackers(player);
        }
        else if (_combatState == TurnState.Blockers)
        {
            _dropZone.PlayerFinishedChoosingBlockers(player);
        }
    }

    public int CheckNumberOfFreeSlots(bool isServer, TurnState state) => _dropZone.GetNumberOfFreeSlots(isServer, state);

    public void ResetHolders() => _dropZone.RpcResetHolders();

    private async UniTaskVoid CombatTransitionAnimation(TurnState state)
    {
        await UniTask.Delay(SorsTimings.wait);

        if (state == TurnState.Attackers) DeclareAttackers();
        else if (state == TurnState.Blockers) DeclareBlockers();
        else if (state == TurnState.CombatCleanUp) CombatCleanUp();
    }
    public void DiscardMoney() => _dropZone.RpcDiscardMoney();
    #endregion
    private void OnDestroy()
    {
        // GameManager.OnGameStart -= PrepareGameStateFile;
        CombatManager.OnCombatStateChanged -= StartCombatPhase;
        BattleZoneEntity.OnEntityDies -= EntityDies;
        EffectHandler.OnPlayerStartSelectTarget -= PlayerStartSelectTarget;
    }

}
