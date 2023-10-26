using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class BlockerArrowHandler : ArrowHandler, IPointerClickHandler
{
    [SerializeField] private CreatureEntity _creature;

    public void OnPointerClick(PointerEventData eventData){
        // return if not in Blockers Phase
        if (CurrentCombatState != CombatState.Blockers) return;

        if (_creature.isOwned) HandleClickedMyCreature();
        else HandleClickedOpponentCreature();
    }
    
    private void HandleClickedMyCreature(){
        
        if (!_creature.CanAct || _creature.IsAttacking || HasTarget) return;
        
        if (!HasOrigin) {
            SpawnArrow();
            _creature.Owner.PlayerChoosesBlocker(_creature);
            return;
        }
        
        _creature.Owner.PlayerRemovesBlocker(_creature);
        RemoveArrow(true);
    }

    private void HandleClickedOpponentCreature(){
        if (!_creature.IsAttacking) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingTarget) return;
        
        clicker.PlayerChoosesAttackerToBlock(_creature);
    }

    public void HandleBlockAttacker(CreatureEntity attacker){
        FoundTarget(attacker.transform.position);
    }
    
    public void ShowOpponentBlocker(GameObject blocker){
        SpawnArrow();
        FoundTarget(blocker.transform.position);
    }
}
