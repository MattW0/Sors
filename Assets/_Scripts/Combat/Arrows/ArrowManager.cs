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
    private TurnState _combatState;
    private PlayerManager _clicker;
    private CombatManager _combatManager;
    private List<CreatureEntity> _creatureGroup = new();
    private Dictionary<int, ArrowRenderer> _floatingArrows = new();
    public bool PlayerIsGroupingCreatures { get; set; }


    private void Awake()
    {
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
        EntityClickHandler.OnEntityClicked += HandleEntityClicked;
        PlayerUI.OnClickedPlayer += HandleClickedPlayerEntity;

        CreatureEntity.OnDeclaredAttack += DeclaredAttack;
        CreatureEntity.OnDeclaredBlock += DeclaredBlock;

        BattleZoneEntity.OnTargetStart += EntityTargetStart;
        BattleZoneEntity.OnTargetFinish += EntityTargetFinish;
    }

    // TODO: Not so pretty
    private async void Start()
    {
        await UniTask.Delay(1000);

        _clicker = PlayerManager.GetLocalPlayer();
        _combatManager = CombatManager.Instance;
    }

    [ClientRpc]
    public void RpcCombatStateChanged(TurnState newState) => _combatState = newState;

    private void HandleClickedPlayerEntity(BattleZoneEntity entity)
    {
        if (_clicker.PlayerIsChoosingTarget) {
            _clicker.PlayerChoosesEntityTarget(entity);
            return;
        }

        if (entity.isOwned) return;

        if (_combatState != TurnState.Attackers) return;
        
        CmdSetGroupTarget(entity, _creatureGroup);
        FinishTargeting(entity.transform.position);
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
            print("Creature already in group");
            _creatureGroup.Remove(creature);
            _floatingArrows[creature.GetInstanceID()].DestroyArrow();
            _floatingArrows.Remove(creature.GetInstanceID());
        } else {
            _creatureGroup.Add(creature);
            var prefab = _combatState == TurnState.Attackers ? attackerArrowPrefab : blockerArrowPrefab;
            SpawnFloatingArrow(prefab, entity.transform, entity.GetInstanceID());
        }
    }

    private void HandleClickedOpponentEntity(BattleZoneEntity entity)
    {
        print("Clicked opponent entity");
        if (!entity.IsTargetable) return;

        CmdSetGroupTarget(entity, _creatureGroup);
        
        FinishTargeting(entity.transform.position);
    }

    private void SpawnArrow(GameObject prefab, Transform origin, Transform target)
    {
        var arrowRenderer = Instantiate(prefab, parentTransform).GetComponent<ArrowRenderer>();
        arrowRenderer.SetOrigin(origin.position);
        arrowRenderer.SetTarget(target.position);
    }

    private void SpawnFloatingArrow(GameObject prefab, Transform origin, int creatureId)
    {
        var arrowRenderer = Instantiate(prefab, parentTransform).GetComponent<ArrowRenderer>();
        arrowRenderer.SetOrigin(origin.position);
        
        _floatingArrows.Add(creatureId, arrowRenderer);
    }

    private void FinishTargeting(Vector3 pos)
    {
        foreach (var _arrow in _floatingArrows.Values) _arrow.SetTarget(pos);

        _creatureGroup.Clear();
        _floatingArrows.Clear();
    }

    [Command(requiresAuthority = false)]
    private void CmdSetGroupTarget(BattleZoneEntity target, List<CreatureEntity> creatureGroup)
    {
        if (_combatState == TurnState.Attackers) _combatManager.PlayerChoosesTargetToAttack(target, creatureGroup); // _clicker.CmdPlayerChoosesTargetToAttack(entity, _creatureGroup);
        else if (_combatState == TurnState.Blockers) _combatManager.PlayerChoosesAttackerToBlock(target.GetComponent<CreatureEntity>(), creatureGroup); // _clicker.CmdPlayerChoosesAttackerToBlock(entity.GetComponent<CreatureEntity>(), _creatureGroup);<
    }

    private void EntityTargetStart(Transform origin, int creatureId) => SpawnFloatingArrow(targetArrowPrefab, origin, creatureId);
    private void EntityTargetFinish(Transform origin, Transform target) => SpawnArrow(targetArrowPrefab, origin, target);
    private void DeclaredAttack(Transform origin, Transform target) => SpawnArrow(attackerArrowPrefab, origin, target);
    private void DeclaredBlock(Transform origin, Transform target) => SpawnArrow(blockerArrowPrefab, origin, target);

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
        EntityClickHandler.OnEntityClicked -= HandleEntityClicked;
        PlayerUI.OnClickedPlayer -= HandleClickedPlayerEntity;
        
        CreatureEntity.OnDeclaredAttack -= DeclaredAttack;
        CreatureEntity.OnDeclaredBlock -= DeclaredBlock;

        BattleZoneEntity.OnTargetStart -= EntityTargetStart;
        BattleZoneEntity.OnTargetFinish -= EntityTargetFinish;
    }
}
