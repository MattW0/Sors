using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class TriggerHandler : NetworkBehaviour
{
    private AbilityQueue _abilityQueue;

    [Header("Helper Fields")]
    private List<TurnState> _turnStateTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, List<Ability>> _presentAbilities = new();

    private void Awake() 
    {
        _abilityQueue = GetComponent<AbilityQueue>();

        TurnManager.OnTurnStateChanged += CheckTurnStateTriggers;
    }

    public void EntityEnters(BattleZoneEntity entity)
    {
        var abilities = entity.CardInfo.abilities;
        if (abilities == null || abilities.Count == 0) return;

        var triggeredAbilities = new List<Ability>();
        foreach (var ability in abilities){
            // Triggers and effects have the same index and a 1-1 relation
            var trigger = ability.trigger;

            // Don't add ETBs to the dict as it resolves immediately and only once
            if (trigger == Trigger.WhenYouPlay){
                _abilityQueue.AddAbility(entity, ability);
                continue;
            }
            
            triggeredAbilities.Add(ability);

            // Beginning of phase triggers
            var state = TriggerToTurnState(trigger);
            if (state != TurnState.None && !_turnStateTriggers.Contains(state)){
                print($"New turn state trigger: {trigger} in state {state}");
                _turnStateTriggers.Add(state);
            }
        }

        _presentAbilities.Add(entity, triggeredAbilities);
    }

    private void CheckTurnStateTriggers(TurnState state)
    {
        // We only search for abilities if the current phase triggers at least one effect
        if (!_turnStateTriggers.Contains(state)) return;

        // TODO: Should probably rebuild this to not search ALL entities
        // Maybe create a dict for each trigger separately and add entities to it
        // Then get the corresponding effect from the entity
        // Also think about how to handle multiple effects on the same trigger
        // 1-many relations
        var phaseTrigger = TurnStateToTrigger(state);
        foreach (var entity in _presentAbilities.Keys){
            foreach (var ability in _presentAbilities[entity]){
                if (ability.trigger == phaseTrigger) _abilityQueue.AddAbility(entity, ability);
            }
        }
    }

    #region Helper Functions
    public void EntityLeaves(BattleZoneEntity entity) => _presentAbilities.Remove(entity);

    private TurnState TriggerToTurnState(Trigger trigger)
    {
        return Enum.TryParse(trigger.ToString(), out TurnState state) ? state : TurnState.None;
        // TODO: Extend triggers to more turn state changes (combat, prevail steps, ...)
        // TurnState s = trigger switch
        // {
        //     Trigger.PhaseSelection => TurnState.PhaseSelection,
        //     Trigger.Draw => TurnState.Draw,
        //     Trigger.Invent => TurnState.Invent,
        //     Trigger.Develop => TurnState.Develop,
        //     Trigger.Attackers => TurnState.Attackers,
        //     Trigger.Blockers => TurnState.Blockers,
        //     Trigger.Recruit => TurnState.Recruit,
        //     Trigger.Deploy => TurnState.Deploy,
        //     Trigger.Prevail => TurnState.Prevail,
        //     Trigger.CleanUp => TurnState.CleanUp,
        //     _ => TurnState.None
        // };

        // return s;
    }

    private Trigger TurnStateToTrigger(TurnState state)
    {
        return Enum.TryParse(state.ToString(), out Trigger trigger) ? trigger : Trigger.None;
        // Trigger trigger = state switch
        // {
        //     TurnState.PhaseSelection => Trigger.PhaseSelection,
        //     TurnState.Draw => Trigger.Draw,
        //     TurnState.Invent => Trigger.Invent,
        //     TurnState.Develop => Trigger.Develop,
        //     TurnState.Attackers => Trigger.Attackers,
        //     TurnState.Blockers => Trigger.Blockers,
        //     TurnState.Recruit => Trigger.Recruit,
        //     TurnState.Deploy => Trigger.Deploy,
        //     TurnState.Prevail => Trigger.Prevail,
        //     TurnState.CleanUp => Trigger.CleanUp,
        //     _ => Trigger.None
        // };

        // return trigger;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnStateChanged -= CheckTurnStateTriggers;
    }
    #endregion
}