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
    private PhasePanel _phasePanel;

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
        _phasePanel = PhasePanel.Instance;

        CombatManager.OnDeclareAttackers += DeclareAttackers;
        CombatManager.OnDeclareBlockers += DeclareBlockers;

    }

    public void AddEntity(PlayerManager owner, PlayerManager opponent, GameObject card, BattleZoneEntity entity) {

        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        // To keep track which card object corresponds to which entity
        _entitiesObjectsCache.Add(entity, card);

        entity.RpcInitializeEntity(owner, opponent, cardInfo);
        dropZone.EntityEntersDropZone(owner, entity);

        entity.OnDeath += EntityDies;
    }

    #region Combat

    private void DeclareAttackers() => dropZone.StartDeclareAttackers(_gameManager.players.Keys.ToList());

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

    private void DeclareBlockers() => dropZone.StartDeclareBlockers(_gameManager.players.Keys.ToList());

    public void BlockersDeclared(PlayerManager player, List<CreatureEntity> playerBlockers) {
        OnBlockersDeclared?.Invoke(player);
    }
    
    private void EntityDies(PlayerManager owner, BattleZoneEntity entity)
    {
        entity.OnDeath -= EntityDies;
        _deadEntities.Add(entity);
    }

    public void CombatCleanUp()
    {
        DestroyArrows();
        foreach (var dead in _deadEntities)
        {
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

        foreach (var attacker in _boardAttackers){
            attacker.RpcRetreatAttacker();
        }
        _boardAttackers.Clear();

        dropZone.CombatCleanUp();
    }

    // public void PlayerSkipsCombatPhase(PlayerManager player) => OnSkipCombatPhase?.Invoke(player);

    #endregion

    #region UI
    public void ShowHolders(bool active){

        if(!active) dropZone.ResetHolders();
        dropZone.RpcHighlightCardHolders();
    }

    public void DisableReadyButton(PlayerManager player){
        _phasePanel.TargetDisableButtons(player.connectionToClient);
    }

    private void DestroyArrows(){
        foreach (var player in _gameManager.players.Keys){
            player.RpcDestroyArrows();
        }
    }

    public void DiscardMoney() => dropZone.RpcDiscardMoney();
    #endregion
    
    private void OnDestroy()
    {
        CombatManager.OnDeclareAttackers -= DeclareAttackers;
        CombatManager.OnDeclareBlockers -= DeclareBlockers;
    }
}
