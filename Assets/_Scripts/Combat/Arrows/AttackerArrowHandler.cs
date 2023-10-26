using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class AttackerArrowHandler : ArrowHandler, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity entity;

    public void OnPointerClick(PointerEventData eventData)
    {
        print($"On pointer click in state {CurrentCombatState}, is Owned: {entity.isOwned}");
        // return if not in Attackers Phase
        if (CurrentCombatState != CombatState.Attackers) return;

        if (entity.isOwned) HandleClickedMyEntity();
        else HandleClickedOpponentTechnology();
    }
    
    private void HandleClickedMyEntity()
    {
        print("Click on my creature");
        if (!entity.Creature.CanAct || HasTarget) return;
        entity.Creature.IsAttacking = !entity.Creature.IsAttacking;
    }

    private void HandleClickedOpponentTechnology()
    {
        // if (!creature.Targetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingTarget) return;
        
        // TODO: Continue with attacking logic
        // clicker.PlayerChoosesAttackerToBlock(entity.Creature);
    }

    private void OnDestroy()
    {
    }
}
