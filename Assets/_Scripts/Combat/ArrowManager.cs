using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cysharp.Threading.Tasks;
using System;

public class ArrowManager : NetworkBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject targetArrowPrefab;
    [SerializeField] private GameObject attackerArrowPrefab;
    [SerializeField] private GameObject blockerArrowPrefab;
    [SerializeField] private GameObject opponentAttackerArrowPrefab;
    [SerializeField] private GameObject opponentBlockerArrowPrefab;
    private TurnState _combatState;
    private PlayerManager _clicker;
    private List<CreatureEntity> _creatureGroup = new();
    private Dictionary<int, ArrowController> _floatingArrows = new();
    [SerializeReference] private Dictionary<int, ArrowController> _combatArrows = new();
    private Dictionary<BattleZoneEntity, List<CreatureEntity>> _currentSelection = new();
    public Dictionary<BattleZoneEntity, List<CreatureEntity>> GetPlayerSelection() => _currentSelection;

    private void Awake()
    {
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
        EntityClickHandler.OnEntityClicked += HandleEntityClicked;
        PlayerEntityUI.OnClickedPlayer += HandleClickedPlayerEntity;

        CreatureEntity.OnOpponentDeclaredAttack += OpponentDeclaredAttack;
        CreatureEntity.OnOpponentDeclaredBlock += OpponentDeclaredBlock;
        CombatClash.OnFinishClash += RpcFinishCombatClash;

        BattleZoneEntity.OnTargetStart += EntityTargetStart;
        BattleZoneEntity.OnTargetFinish += EntityTargetFinish;
    }

    // TODO: Not so pretty
    private async void Start()
    {
        await UniTask.Delay(1000);

        _clicker = PlayerManager.GetLocalPlayer();
    }

    [ClientRpc]
    public void RpcCombatStateChanged(TurnState newState)
    {
        _combatState = newState;  
        _currentSelection.Clear();
    } 

    [TargetRpc]
    public void TargetResetArrows(NetworkConnection conn)
    {
        foreach (var (_, creatureList) in _currentSelection) 
        {
            foreach (var creature in creatureList) 
            {
                _combatArrows[creature.ID].DestroyArrow();
                creature.CanAct = true;
            }
        }
        
        foreach (var _arrow in _floatingArrows.Values) _arrow.DestroyArrow();
        
        _floatingArrows.Clear();
        _combatArrows.Clear();
        _currentSelection.Clear();
    }

    [ClientRpc]
    private void RpcFinishCombatClash(int id)
    {
        if (!_combatArrows.ContainsKey(id)) return;
        
        _combatArrows[id].DestroyArrow();
        _combatArrows.Remove(id);
    }

    private void HandleClickedPlayerEntity(BattleZoneEntity playerEntity)
    {
        // Can click either player
        if (_clicker.PlayerIsChoosingTarget) _clicker.PlayerChoosesEntityTarget(playerEntity);
        else if (_combatState == TurnState.Attackers && ! playerEntity.isOwned) GroupOnTarget(playerEntity);
    }

    private void HandleEntityClicked(BattleZoneEntity entity)
    {
        // Targeting logic
        if (_clicker.PlayerIsChoosingTarget)
        {
            if (entity.IsTargetable) _clicker.PlayerChoosesEntityTarget(entity);
            return;
        }

        // Combat logic (attacking and blocking)
        if (entity.isOwned) HandleClickedMyEntity(entity);
        else HandleClickedOpponentEntity(entity);
    }

    private void HandleClickedMyEntity(BattleZoneEntity entity)
    {
        if (entity.cardType != CardType.Creature) return;

        var creature = entity.GetComponent<CreatureEntity>();
        if (!creature.CanAct) return;
        
        if (_creatureGroup.Contains(creature))
        {
            _creatureGroup.Remove(creature);
            _floatingArrows[creature.ID].DestroyArrow();
            _floatingArrows.Remove(creature.ID);
            _combatArrows.Remove(creature.ID);
        } else {
            _creatureGroup.Add(creature);
            var prefab = _combatState == TurnState.Attackers ? attackerArrowPrefab : blockerArrowPrefab;
            SpawnFloatingArrow(prefab, entity.transform, entity.ID);
        }
    }

    private void SpawnFloatingArrow(GameObject prefab, Transform origin, int id)
    {
        var arrowRenderer = Instantiate(prefab, parentTransform, true).GetComponent<ArrowController>();
        arrowRenderer.SetOrigin(origin.position);
        
        _combatArrows.Add(id, arrowRenderer);
        if(_floatingArrows.ContainsKey(id)) _floatingArrows[id] = arrowRenderer;
        else _floatingArrows.Add(id, arrowRenderer);
    }

    private void HandleClickedOpponentEntity(BattleZoneEntity entity)
    {
        if (!entity.IsTargetable) return;

        if (_floatingArrows.Count == 0) return;

        GroupOnTarget(entity);
    }

    private void GroupOnTarget(BattleZoneEntity entity)
    {
        if (_currentSelection.ContainsKey(entity)) _currentSelection[entity].AddRange(_creatureGroup);
        else _currentSelection.Add(entity, new List<CreatureEntity>(_creatureGroup));

        // Disable creatures from acting
        foreach (var creature in _creatureGroup) creature.CanAct = false;

        // Set arrows to target
        foreach (var _arrow in _floatingArrows.Values) _arrow.SetTarget(entity.transform.position);

        _creatureGroup.Clear();
        _floatingArrows.Clear();
    }

    private void OpponentDeclaredAttack(BattleZoneEntity origin, BattleZoneEntity target)
    {
        // print("ArrowManager: Declared attack");
        if (_combatArrows.ContainsKey(origin.ID)) return;

        var arrow = SpawnArrowFromOpponent(opponentAttackerArrowPrefab, origin.transform, target.transform);
        _combatArrows[origin.ID] = arrow;
    }
    private void OpponentDeclaredBlock(BattleZoneEntity origin, BattleZoneEntity target)
    {
        // print("ArrowManager: Declared block");
        if (_combatArrows.ContainsKey(origin.ID)) return;

        var arrow = SpawnArrowFromOpponent(opponentBlockerArrowPrefab, origin.transform, target.transform);
        _combatArrows[origin.ID] = arrow;
    }

    private ArrowController SpawnArrowFromOpponent(GameObject prefab, Transform origin, Transform target)
    {
        var arrowRenderer = Instantiate(prefab, parentTransform).GetComponent<ArrowController>();
        
        arrowRenderer.SetOrigin(origin.position);
        arrowRenderer.SetTarget(target.position);

        return arrowRenderer;
    }

    private void EntityTargetStart(Transform origin, int creatureId) => SpawnFloatingArrow(targetArrowPrefab, origin, creatureId);
    private void EntityTargetFinish(bool isOwned, Transform origin, Transform target)
    {
        if (isOwned) {
            foreach (var _arrow in _floatingArrows.Values) _arrow.SetTarget(target.position);
            _floatingArrows.Clear();
        } else {
            SpawnArrowFromOpponent(targetArrowPrefab, origin, target);
        }
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
        EntityClickHandler.OnEntityClicked -= HandleEntityClicked;
        PlayerEntityUI.OnClickedPlayer -= HandleClickedPlayerEntity;
        
        CreatureEntity.OnOpponentDeclaredAttack -= OpponentDeclaredAttack;
        CreatureEntity.OnOpponentDeclaredBlock -= OpponentDeclaredBlock;
        CombatClash.OnFinishClash -= RpcFinishCombatClash;

        BattleZoneEntity.OnTargetStart -= EntityTargetStart;
        BattleZoneEntity.OnTargetFinish -= EntityTargetFinish;

        TestEntity.OnEntityClicked -= SpawnFloatingArrow;
    }

}
