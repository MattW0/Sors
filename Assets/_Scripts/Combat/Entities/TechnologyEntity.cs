using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Mirror;

public class TechnologyEntity : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private AttackerArrowHandler _attackerArrowHandler;
    [SerializeField] private CombatState _combatState;

    public void OnPointerClick(PointerEventData eventData){
        
        // Right click -> card zoom (handled in BattleZoneEntity)
        if (eventData.button == PointerEventData.InputButton.Right) {
            return;
        }

        if (!isOwned) return;

        // only in Attackers since blockerArrowHandler handles Blockers phase
        if (_combatState != CombatState.Attackers) return;
        
        // TODO: stuff here
    }
}
