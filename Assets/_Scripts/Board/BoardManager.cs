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
    [SerializeField] private CardEffectsHandler _cardEffectsHandler;
    private DropZoneManager _dropZone;
    [SerializeField] private PhasePanel _phasePanel;

    // Entities, corresponding card object
    private Dictionary<BattleZoneEntity, GameObject> _entitiesObjectsCache = new();
    private List<BattleZoneEntity> _deadEntities = new();
    private List<CreatureEntity> _boardAttackers = new();
    public List<CreatureEntity> GetBoardAttackers() => _boardAttackers;

    public static event Action<PlayerManager> OnAttackersDeclared;
    public static event Action<PlayerManager> OnBlockersDeclared;

    private void Awake() {
        if (!Instance) Instance = this;

        _gameManager = GameManager.Instance;
        _dropZone = DropZoneManager.Instance;
    }

    public void AddEntity(PlayerManager owner, PlayerManager opponent, GameObject card, BattleZoneEntity entity) {
        
        // print("Adding entity, owner: " + owner.name + ", opponent: " + opponent.name + ", card: " + card.name + ", entity: " + entity.name);
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        // To keep track which card object corresponds to which entity
        _entitiesObjectsCache.Add(entity, card);

        entity.RpcInitializeEntity(owner, opponent, cardInfo);
        _dropZone.EntityEntersDropZone(owner, entity);
        _cardEffectsHandler.CardIsPlayed(owner, entity, cardInfo);

        // entity.OnDeath += EntityDies;
    }

    #region Combat

    public void StartCombatPhase(CombatState state){
        StartCoroutine(CombatTransitionAnimation(state));
    }

    private IEnumerator CombatTransitionAnimation(CombatState state){
        _phasePanel.RpcStartCombatPhase(state);
        yield return new WaitForSeconds(1f);
        if (state == CombatState.Attackers) DeclareAttackers();
        else if (state == CombatState.Blockers) DeclareBlockers();
    }

    private void DeclareAttackers() => _dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList());

    public void AttackersDeclared(PlayerManager player, List<CreatureEntity> playerAttackers) {
        foreach (var a in playerAttackers) _boardAttackers.Add(a);
        OnAttackersDeclared?.Invoke(player);
    }

    public void ShowOpponentAttackers() {
        // Share attacker state accross clients
        foreach (var creature in _boardAttackers){
            creature.IsAttacking = true;
            creature.RpcOpponentCreatureIsAttacker();
        }
    }

    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());

    public void BlockersDeclared(PlayerManager player, List<CreatureEntity> playerBlockers) {
        OnBlockersDeclared?.Invoke(player);
    }
    
    public void EntityDies(BattleZoneEntity entity)
    {
        _deadEntities.Add(entity);
        // entity.OnDeath -= EntityDies;
    }

    public void CombatCleanUp()
    {
        DestroyArrows();
        ClearDeadEntities();

        _boardAttackers.Clear();
        _dropZone.CombatCleanUp();
    }

    public void ClearDeadEntities(){
        foreach (var dead in _deadEntities)
        {
            _dropZone.EntityLeavesPlayZone(dead);
            _cardEffectsHandler.EntityDies(dead);

            if(dead.cardType == CardType.Creature){
                var creature = dead.GetComponent<CreatureEntity>();
                _boardAttackers.Remove(creature);
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

    // public void PlayerSkipsCombatPhase(PlayerManager player) => OnSkipCombatPhase?.Invoke(player);

    #endregion

    public void BoardCleanUp(){
        // _phasePanel.RpcStartBoardCleanUp();
        _dropZone.DevelopmentsLooseHealth();
        _phasePanel.RpcClearPlayerChoiceHighlights();
        ClearDeadEntities();
    }

    #region UI
    public void ShowHolders(bool active){

        if(!active) { 
            _dropZone.RpcResetHolders();
        } else {
            var turnState = TurnManager.GetTurnState();
            _dropZone.RpcHighlightCardHolders(turnState);
        }
    }

    public void DisableReadyButton(PlayerManager player){
        _phasePanel.TargetDisableCombatButtons(player.connectionToClient);
    }

    private void DestroyArrows(){
        foreach (var player in _gameManager.players.Keys){
            player.RpcDestroyArrows();
        }
    }

    public void DiscardMoney() => _dropZone.RpcDiscardMoney();
    #endregion
}
