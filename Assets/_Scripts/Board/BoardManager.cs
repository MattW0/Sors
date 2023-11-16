using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEditor;


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

    // Events
    public static event Action<PlayerManager> OnAttackersDeclared;
    public static event Action<PlayerManager> OnBlockersDeclared;

    private void Awake()
    {
        if (!Instance) Instance = this;

        _gameManager = GameManager.Instance;
        _dropZone = DropZoneManager.Instance;
    }

    public void AddEntity(PlayerManager owner, PlayerManager opponent,
                          GameObject card, BattleZoneEntity entity, bool isPlayed = true) 
    {
        // Initialize and keep track which card object corresponds to which entity
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        entity.RpcInitializeEntity(owner, opponent, cardInfo);
        _entitiesObjectsCache.Add(entity, card);
        // Can be loaded from json at the beginning
        if(isPlayed) _cardEffectsHandler.CardIsPlayed(owner, entity, cardInfo);

        StartCoroutine(_dropZone.EntitySpawns(owner, card, entity));
    }

    #region Effects

    public void FindTargets(BattleZoneEntity entity, EffectTarget target)
    {
        print(entity.Title + " - looking for target: " + target);
        var owner = entity.Owner;

        // TODO : Check if it makes sense to continue effects handler from here
        // Also need to add other possible targets (player, ...)
        if (target.Equals(EffectTarget.Entity)){
            // _cardEffectsHandler.Continue = true;
            _dropZone.ShowAbilityTargets(owner, target);
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
        yield return new WaitForSeconds(1f);

        if (state == CombatState.Attackers) DeclareAttackers();
        else if (state == CombatState.Blockers) DeclareBlockers();
        else if (state == CombatState.CleanUp) CombatCleanUp();
    }

    private void DeclareAttackers() {

        if (_gameManager.singlePlayer) 
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

    public void AttackersDeclared(PlayerManager player)
    {
        OnAttackersDeclared?.Invoke(player);
    }
    
    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());
    public void BlockersDeclared(PlayerManager player) => OnBlockersDeclared?.Invoke(player);
    
    public void EntityDies(BattleZoneEntity entity)
    {
        // Catch exception where entity was already dead and received more damage
        if (_deadEntities.Contains(entity)) return;

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
            _dropZone.DevelopmentsLooseHealth();
            ClearDeadEntities();
        }
    }

    public void ClearDeadEntities()
    {
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
}
