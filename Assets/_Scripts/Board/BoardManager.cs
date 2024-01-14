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
    private DropZoneManager _dropZone;
    [SerializeField] private CardEffectsHandler _cardEffectsHandler;
    [SerializeField] private PhasePanel _phasePanel;

    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsCache = new();
    private List<BattleZoneEntity> _deadEntities = new();
    private CombatState _combatState;
    private GameState _gameState;

    // Events
    public static event Action OnCombatStart;
    public static event Action OnCombatEnd;
    public static event Action<PlayerManager> OnAttackersDeclared;
    public static event Action<PlayerManager> OnBlockersDeclared;

    private void Awake()
    {
        if (!Instance) Instance = this;

        CombatManager.OnCombatStateChanged += StartCombatPhase;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _dropZone = DropZoneManager.Instance;

        // GameManager.OnGameStart += PrepareGameStateFile;
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

    public void FindTargets(BattleZoneEntity entity, EffectTarget target)
    {
        var owner = entity.Owner;
        print($"Player {owner.PlayerName} - looking for target: {target}");

        // TODO : Check if it makes sense to continue effects handler from here
        // Also need to add other possible targets (player, ...)
        if (target.Equals(EffectTarget.Entity)){
            // _cardEffectsHandler.Continue = true;
            _dropZone.EntitiesAreTargetable(owner);
            return;
        }
    }

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

        if (state == CombatState.Attackers) {
            DeclareAttackers();
            RpcStartAttackers();
        } else if (state == CombatState.Blockers) {
            DeclareBlockers();
            RpcStartBlockers();
        }
        else if (state == CombatState.CleanUp) CombatCleanUp();
    }

    [ClientRpc]
    private void RpcStartAttackers() => OnCombatStart?.Invoke();
    [ClientRpc]
    private void RpcStartBlockers() => OnCombatEnd?.Invoke();

    private void DeclareAttackers() {

        if (_gameManager.isSinglePlayer)
        {
            _dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList()[0], null);
            return;
        }

        foreach (var player in _gameManager.players.Keys)
        {
            var opponentEntity = _gameManager.GetOpponent(player).GetComponent<BattleZoneEntity>();
            _dropZone.StartDeclareAttackers(player, opponentEntity);
        }
    }

    public void AttackersDeclared(PlayerManager player) => OnAttackersDeclared?.Invoke(player);
    
    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());
    public void BlockersDeclared(PlayerManager player) => OnBlockersDeclared?.Invoke(player);
    
    public void EntityDies(BattleZoneEntity entity)
    {
        // Catch exception where entity was already dead and received more damage
        if (_deadEntities.Contains(entity)) return;

        print($"{entity.Title} dies");
        _deadEntities.Add(entity);
    }

    private void CombatCleanUp()
    {
        ClearDeadEntities();

        // Reset entities and destroy blocker arrows
        _dropZone.CombatCleanUp();
    }
    #endregion

    public void BoardCleanUp(bool endOfTurn)
    {    
        ClearDeadEntities();
        _dropZone.DestroyTargetArrows();

        if(endOfTurn){
            _dropZone.TechnologiesLooseHealth();
            ClearDeadEntities();
            if(isServer) SaveGameState();
        }
    }

    public void ClearDeadEntities()
    {
        // print("_deadEntities : " + _deadEntities.Count);
        foreach (var dead in _deadEntities)
        {
            _dropZone.EntityLeavesPlayZone(dead);
            _cardEffectsHandler.EntityDies(dead);

            if(dead.cardType == CardType.Creature){
                var creature = dead.GetComponent<CreatureEntity>();
            }

            // Move the card object to discard pile
            var cardObject = _entitiesObjectsCache[dead];
            dead.Owner.discard.Add(cardObject.GetComponent<CardStats>().cardInfo);
            dead.Owner.RpcMoveCard(cardObject, CardLocation.PlayZone, CardLocation.Discard);

            _entitiesObjectsCache.Remove(dead);
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();
    }

    [Server]
    public void PrepareGameStateFile()
    {
        _gameState = new GameState(_gameManager.players.Count);
        int i = 0;
        foreach (var player in _gameManager.players.Keys){
            _gameState.players[i] = new Player(player.PlayerName, player.isLocalPlayer);
            i++;
        }

        SaveGameState();
    }

    [Server]
    private void SaveGameState()
    {
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

    public void DisableReadyButton(PlayerManager player) => _phasePanel.TargetDisableCombatButtons(player.connectionToClient);
    public void DiscardMoney() => _dropZone.RpcDiscardMoney();
    #endregion
    private void OnDestroy()
    {
        // GameManager.OnGameStart -= PrepareGameStateFile;
        CombatManager.OnCombatStateChanged -= StartCombatPhase;
    }

}
