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

        CombatManager.OnDeclareBlockers += DeclareBlockers;

    }

    public void AddEntity(PlayerManager owner, PlayerManager opponent, GameObject card, BattleZoneEntity entity) {

        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        // To keep track which card object corresponds to which entity
        _entitiesObjectsCache.Add(entity, card);

        entity.RpcInitializeEntity(owner, opponent, cardInfo);
        _dropZone.EntityEntersDropZone(owner, entity);
        _cardEffectsHandler.CardIsPlayed(owner, entity, cardInfo);

        // entity.OnDeath += EntityDies;
    }

    #region Combat

    public void StartCombat(){
        StartCoroutine(CombatTransitionAnimation(CombatState.Attackers));
    }

    private IEnumerator CombatTransitionAnimation(CombatState state){
        _phasePanel.RpcStartCombatPhase(state);
        yield return new WaitForSeconds(1f);
        DeclareAttackers();
    }

    private void DeclareAttackers() => _dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList());

    public void AttackersDeclared(PlayerManager player, List<CreatureEntity> playerAttackers) {
        // Is 0 for auto-skip or no attackers declared
        if (playerAttackers.Count > 0) {
            foreach (var a in playerAttackers) _boardAttackers.Add(a);
        }

        OnAttackersDeclared?.Invoke(player);
    }

    public void ShowOpponentAttackers() {
        // Share attacker state accross clients
        foreach (var creature in _boardAttackers){
            creature.IsAttacking = true;
            creature.RpcIsAttacker();
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

        foreach (var attacker in _boardAttackers){
            attacker.RpcRetreatAttacker();
        }
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
            dead.Owner.discard.Add(_entitiesObjectsCache[dead].GetComponent<CardStats>().cardInfo);
            dead.Owner.RpcMoveCard(_entitiesObjectsCache[dead],
                CardLocation.PlayZone, CardLocation.Discard);

            _entitiesObjectsCache.Remove(dead);
            NetworkServer.Destroy(dead.gameObject);
        }
        _deadEntities.Clear();
    }

    // public void PlayerSkipsCombatPhase(PlayerManager player) => OnSkipCombatPhase?.Invoke(player);

    #endregion

    public void DevelopmentsLooseHealth(){
        _dropZone.DevelopmentsLooseHealth();
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
    
    private void OnDestroy()
    {
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
