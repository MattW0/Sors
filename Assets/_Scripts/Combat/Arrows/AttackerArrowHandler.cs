using System;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class AttackerArrowHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CreatureEntity creature;
    private CombatState _currentState;
    private bool _hasTarget;

    private void Awake()
    {
    }

    
    
    

    public void OnPointerClick(PointerEventData eventData)
    {
        // return if not in Attackers Phase
        if (_currentState != CombatState.Attackers) return;

        if (creature.isOwned) HandleClickedMyEntity();
        else HandleClickedOpponentTechnology();
    }
    
    private void HandleClickedMyEntity()
    {
        if (!creature.CanAct || _hasTarget) return;
        
    }

    private void HandleClickedOpponentTechnology()
    {
        // if (!creature.Targetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingTarget) return;
        
        clicker.PlayerChoosesAttackerToBlock(creature);
    }

    private void OnDestroy()
    {
    }
}
