using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class BlockerArrowHandler : ArrowHandler, IPointerClickHandler
{
    private CreatureEntity _creature;

    private void Awake()
    {
        _creature = gameObject.GetComponent<CreatureEntity>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_creature) return;
        // return if not in Blockers Phase
        if (CurrentCombatState != CombatState.Blockers) return;

        if (_creature.isOwned) HandleClickedMyCreature();
        else HandleClickedOpponentCreature();
    }

    private void HandleClickedMyCreature()
    {
        if (!_creature.CanAct || _creature.IsAttacking || HasTarget) return;

        if (!HasOrigin)
        {
            SpawnArrow();
            _creature.IsBlocking = true;
            _creature.Owner.PlayerChoosesBlocker(_creature);
        } else {
            RemoveArrow(true);
            _creature.IsBlocking = false;
            _creature.Owner.PlayerRemovesBlocker(_creature);
        }
    }

    private void HandleClickedOpponentCreature()
    {
        if (!_creature.IsAttacking) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingBlock) return;

        clicker.PlayerChoosesAttackerToBlock(_creature);
    }
}
