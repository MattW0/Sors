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
    private List<CreatureEntity> _boardAttackers = new();
    public List<CreatureEntity> GetBoardAttackers() => _boardAttackers;

    public static event Action<PlayerManager> OnAttackersDeclared;
    public static event Action<PlayerManager> OnBlockersDeclared;

    private void Awake()
    {
        if (!Instance) Instance = this;

        _gameManager = GameManager.Instance;
        _dropZone = DropZoneManager.Instance;
    }

    public IEnumerator AddEntity(PlayerManager owner, PlayerManager opponent, 
                          GameObject card, BattleZoneEntity entity, bool isPlayed = true) 
    {
        // Initialize
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        entity.RpcInitializeEntity(owner, opponent, cardInfo);

        // Need to await RPC for initialization
        while (!entity.Owner) yield return new WaitForSeconds(0.1f);
        
        // TODO: Add animations for ETB here
        yield return new WaitForSeconds(0.1f);
        

        _dropZone.EntityEntersDropZone(owner, entity);
        // Can be loaded from json at the beginning
        if(isPlayed) _cardEffectsHandler.CardIsPlayed(owner, entity, cardInfo);

        // To keep track which card object corresponds to which entity
        _entitiesObjectsCache.Add(entity, card);
        // StartCoroutine(EntityEntersDropzone(owner, cardInfo, entity, isPlayed));

        yield return null;
    }

    private IEnumerator EntityEntersDropzone(PlayerManager owner, CardInfo cardInfo, 
                                             BattleZoneEntity entity, bool isPlayed){
        
        
        
        yield return null;
    }

    private void EntityEntersDropZone(){
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
            _dropZone.ShowTargets(owner, target);
            return;
        }
    }

    #endregion

    #region Combat

    public void StartCombatPhase(CombatState state) => StartCoroutine(CombatTransitionAnimation(state));

    private IEnumerator CombatTransitionAnimation(CombatState state)
    {
        _phasePanel.RpcStartCombatPhase(state);
        yield return new WaitForSeconds(1f);
        if (state == CombatState.Attackers) DeclareAttackers();
        else if (state == CombatState.Blockers) DeclareBlockers();
    }

    private void DeclareAttackers() => _dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList());

    public void AttackersDeclared(PlayerManager player, List<CreatureEntity> playerAttackers)
    {
        foreach (var a in playerAttackers) _boardAttackers.Add(a);
        OnAttackersDeclared?.Invoke(player);
    }

    public void ShowOpponentAttackers() 
    {
        // Share attacker state accross clients
        foreach (var creature in _boardAttackers){
            creature.IsAttacking = true;
            creature.RpcOpponentCreatureIsAttacker();
        }
    }

    private void DeclareBlockers() => _dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());
    public void BlockersDeclared(PlayerManager player) => OnBlockersDeclared?.Invoke(player);
    
    public void EntityDies(BattleZoneEntity entity)
    {
        // Catch exception where entity was already dead and received more damage
        if (_deadEntities.Contains(entity)) return;

        _deadEntities.Add(entity);
    }

    public void CombatCleanUp()
    {
        ClearDeadEntities();
        _boardAttackers.Clear();

        // Reset entities and destroy blocker arrows
        _dropZone.CombatCleanUp();
    }
    #endregion

    public void BoardCleanUp(bool endOfTurn)
    {    
        ClearDeadEntities();
        _dropZone.DestroyTargetArrows();

        if(! endOfTurn) return;
        _dropZone.DevelopmentsLooseHealth();
    }

    public void ClearDeadEntities()
    {
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

    #region UI
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
