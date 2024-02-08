using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEditor;
using SorsGameState;


public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; private set; }
    private GameManager _gameManager;
    private CombatManager _combatManager;
    private DropZoneManager _dropZone;
    [SerializeField] private CardEffectsHandler _cardEffectsHandler;
    [SerializeField] private PhasePanel _phasePanel;

    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsCache = new();
    private List<BattleZoneEntity> _deadEntities = new();
    private CombatState _combatState;
    private GameState _gameState;

    private void Awake()
    {
        if (!Instance) Instance = this;

        CombatManager.OnCombatStateChanged += StartCombatPhase;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _combatManager = CombatManager.Instance;
        _dropZone = DropZoneManager.Instance;
    }

    public void PlayEntities(Dictionary<GameObject, BattleZoneEntity> entities) 
    {
        foreach(var (card, entity) in entities){
            // Initialize and keep track which card object corresponds to which entity
            _entitiesObjectsCache.Add(entity, card);
        }

        // Check for ETB and if phase start trigger gets added to phases being tracked
        StartCoroutine(_cardEffectsHandler.CardsArePlayed(entities.Values.ToList()));

        // Move entities to holders and card into played zone
        StartCoroutine(_dropZone.EntitiesEnterDropZone(entities));
    }

    #region Effects

    public void PlayerStartSelectTarget(BattleZoneEntity entity, Ability ability)
    {
        var owner = entity.Owner;

        // Target rpc let triggering player choose
        owner.TargetPlayerStartChooseTarget();
        entity.TargetSpawnTargetArrow(owner.connectionToClient);

        _dropZone.RpcEntitiesAreTargetable(ability.target);
    }

    public void ResetTargeting() => _dropZone.RpcResetTargeting();

    #endregion

    #region Combat

    public void StartCombatPhase(CombatState state)
    {
        _combatState = state;
        _phasePanel.RpcStartCombatPhase(state);

        StartCoroutine(CombatTransitionAnimation(state));
    }

    private IEnumerator CombatTransitionAnimation(CombatState state)
    {
        yield return new WaitForSeconds(SorsTimings.turnStateTransition);

        if (state == CombatState.Attackers) DeclareAttackers(); 
        else if (state == CombatState.Blockers) DeclareBlockers();
        else if (state == CombatState.CleanUp) CombatCleanUp();
    }

    private void DeclareAttackers() => _dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList());
    public void AttackersDeclared(PlayerManager player)
    {
        DisableReadyButton(player);
        _combatManager.PlayerDeclaredAttackers(player);
    }
    
    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());
    public void BlockersDeclared(PlayerManager player)
    {
        DisableReadyButton(player);
        _combatManager.PlayerDeclaredBlockers(player);
    }
    
    public void EntityDies(BattleZoneEntity entity)
    {
        // Catch exception where entity was already dead and received more damage
        if (_deadEntities.Contains(entity)) return;

        print($"{entity.Title} dies");
        _deadEntities.Add(entity);
    }

    private void CombatCleanUp()
    {
        // Reset entities and destroy blocker arrows
        _dropZone.CombatCleanUp();

        ClearDeadEntities();
    }
    #endregion

    public void BoardCleanUp(List<CardInfo>[] scriptableTiles, bool endOfTurn)
    {
        ClearDeadEntities();
        _dropZone.DestroyTargetArrows();

        if(endOfTurn){
            _dropZone.TechnologiesLooseHealth();
            ClearDeadEntities();
            if(isServer) SaveGameState(scriptableTiles);
        }
    }

    public void ClearDeadEntities()
    {
        // print("_deadEntities : " + _deadEntities.Count);
        foreach (var dead in _deadEntities)
        {
            _dropZone.EntityLeavesPlayZone(dead);
            _cardEffectsHandler.EntityDies(dead);

            // Move the card object to discard pile
            var cardObject = _entitiesObjectsCache[dead];
            dead.Owner.discard.Add(cardObject.GetComponent<CardStats>().cardInfo);
            dead.Owner.RpcMoveCard(cardObject, CardLocation.PlayZone, CardLocation.Discard);

            // Somehow NetworkServer.Destroy(this) destroys the GO but does not call OnDestroy(),
            // Thus, do it here manually to prevent null references when events are triggered
            dead.RpcUnsubscribeEvents();

            _entitiesObjectsCache.Remove(dead);
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();
    }

    [Server]
    public void PrepareGameStateFile(List<CardInfo>[] scriptableTiles)
    {
        _gameState = new GameState(_gameManager.players.Count);

        int i = 0;
        foreach (var player in _gameManager.players.Keys){
            _gameState.players[i] = new Player(player.PlayerName, player.isLocalPlayer);
            i++;
        }

        SaveGameState(scriptableTiles);
    }

    [Server]
    private void SaveGameState(List<CardInfo>[] scriptableTiles)
    {
        _gameState.market.SaveMarketState(scriptableTiles);

        int i = 0;
        foreach(var player in _gameManager.players.Keys){
            _gameState.players[i].SavePlayerState(player);
            
            var (creatures, technologies) = _dropZone.GetPlayerEntities(player);

            _gameState.players[i].entities.creatures.Clear();
            _gameState.players[i].entities.technologies.Clear();

            foreach(var entity in creatures){
                var e = new SorsGameState.Entity(entity.CardInfo, entity.Health, entity.Attack);
                _gameState.players[i].entities.creatures.Add(e);
            }
            foreach(var entity in technologies){
                var e = new SorsGameState.Entity(entity.CardInfo, entity.Health);
                _gameState.players[i].entities.technologies.Add(e);
            }

            i++;
        }
        
        _gameState.SaveState(_gameManager.turnNumber);
    }

    #region UI
    public void PlayerPressedReadyButton(PlayerManager player){
        if (_combatState == CombatState.Attackers)
        {
            _dropZone.PlayerFinishedChoosingAttackers(player);
        }
        else if (_combatState == CombatState.Blockers)
        {
            _dropZone.PlayerFinishedChoosingBlockers(player);
        }
    }

    public void ShowHolders(bool active)
    {
        if(active) { 
            var turnState = TurnManager.GetTurnState();
            _dropZone.RpcHighlightCardHolders(turnState);
        } else {
            _dropZone.RpcResetHolders();
        }
    }

    private void DisableReadyButton(PlayerManager player) => _phasePanel.TargetDisableCombatButtons(player.connectionToClient);
    public void DiscardMoney() => _dropZone.RpcDiscardMoney();
    #endregion
    private void OnDestroy()
    {
        // GameManager.OnGameStart -= PrepareGameStateFile;
        CombatManager.OnCombatStateChanged -= StartCombatPhase;
    }

}
