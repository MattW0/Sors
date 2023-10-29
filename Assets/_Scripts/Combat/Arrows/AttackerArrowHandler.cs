using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class AttackerArrowHandler : ArrowHandler, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity entity;
    private CreatureEntity _creature;

    private void Awake()
    {
        _creature = gameObject.GetComponent<CreatureEntity>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // print($"On pointer click in state {CurrentCombatState}, is Owned: {entity.isOwned}");
        // return if not in Attackers Phase
        if (CurrentCombatState != CombatState.Attackers) return;

        if (entity.isOwned) HandleClickedMyEntity();
        else HandleClickedOpponentTechnology();
    }

    private void HandleClickedMyEntity()
    {
        if (!_creature.CanAct || HasTarget) return;

        if (!_creature.IsAttacking)
        {
            SpawnArrow();
            _creature.IsAttacking = true;
            entity.Owner.PlayerChoosesAttacker(entity.Creature);
        }
        else
        {
            RemoveArrow(true);
            _creature.IsAttacking = false;
            entity.Owner.PlayerRemovesAttacker(entity.Creature);
        }
    }

    private void HandleClickedOpponentTechnology()
    {
        // if (!entity.IsTargetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingTarget) return;

        // TODO: Continue with attacking logic
        clicker.PlayerChoosesTargetToAttack(entity);
    }

    public void ShowOpponentAttacker(CreatureEntity attacker)
    {
        SpawnArrow();
        FoundTarget(attacker.transform.position);
    }

    private void OnDestroy()
    {
    }
}
