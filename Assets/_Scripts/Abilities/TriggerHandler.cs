using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TriggerHandler : NetworkBehaviour
{
    public static TriggerHandler Instance { get; private set; }
    private AbilityQueue _abilityQueue;

    [Header("Helper Fields")]
    private List<TurnState> _turnStateTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, List<Ability>> _presentAbilities = new();

    private void Awake() {
        if (!Instance) Instance = this;
        _abilityQueue = GetComponent<AbilityQueue>();

        TurnManager.OnPhaseChanged += CheckTurnStateTriggers;
    }

    public IEnumerator CardsArePlayed(List<BattleZoneEntity> entities)
    {
        // Wait for card initialization
        yield return new WaitForSeconds(0.1f);

        foreach (var entity in entities){
            var abilities = entity.CardInfo.abilities;
            if (abilities == null || abilities.Count == 0) continue;

            var presentAbilities = new List<Ability>();
            foreach (var ability in abilities){
                // Triggers and effects have the same index and a 1-1 relation
                var trigger = ability.trigger;

                // Don't add ETBs to the dict as it resolves immediately and only once
                if (trigger == Trigger.When_you_play){
                    _abilityQueue.AddAbility(entity, ability);
                    continue;
                }
                
                presentAbilities.Add(ability);

                // Beginning of phase triggers
                var state = TriggerToTurnState(trigger);
                if (state != TurnState.None && !_turnStateTriggers.Contains(state)){
                    print($"New turn state trigger: {trigger} in state {state}");
                    _turnStateTriggers.Add(state);
                }
            }

            _presentAbilities.Add(entity, presentAbilities);
        }
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
    public void EntityDies(BattleZoneEntity entity) => _presentAbilities.Remove(entity);

    private TurnState TriggerToTurnState(Trigger trigger)
    {
        // TODO: Extend triggers to more turn state changes (combat, prevail steps, ...)
        TurnState state = trigger switch
        {
            Trigger.Beginning_Turn => TurnState.PhaseSelection,
            Trigger.Beginning_Draw => TurnState.Draw,
            Trigger.Beginning_Invent => TurnState.Invent,
            Trigger.Beginning_Develop => TurnState.Develop,
            Trigger.Beginning_Combat => TurnState.Combat,
            Trigger.Beginning_Recruit => TurnState.Recruit,
            Trigger.Beginning_Deploy => TurnState.Deploy,
            Trigger.Beginning_Prevail => TurnState.Prevail,
            Trigger.Beginning_CleanUp => TurnState.CleanUp,
            _ => TurnState.None
        };

        return state;
    }

    private Trigger TurnStateToTrigger(TurnState state)
    {
        Trigger trigger = state switch
        {
            TurnState.PhaseSelection => Trigger.Beginning_Turn,
            TurnState.Draw => Trigger.Beginning_Draw,
            TurnState.Invent => Trigger.Beginning_Invent,
            TurnState.Develop => Trigger.Beginning_Develop,
            TurnState.Combat => Trigger.Beginning_Combat,
            TurnState.Recruit => Trigger.Beginning_Recruit,
            TurnState.Deploy => Trigger.Beginning_Deploy,
            TurnState.Prevail => Trigger.Beginning_Prevail,
            TurnState.CleanUp => Trigger.Beginning_CleanUp,
            _ => Trigger.None
        };

        return trigger;
    }

    private void Destroy()
    {
        TurnManager.OnPhaseChanged -= CheckTurnStateTriggers;
    }
    #endregion
}