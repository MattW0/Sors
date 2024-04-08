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
    public bool QueueResolving { get; private set; }

    [Header("Helper Fields")]
    private List<Phase> _phaseTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, List<Ability>> _presentAbilities = new();

    private void Awake() {
        if (!Instance) Instance = this;
        _abilityQueue = GetComponent<AbilityQueue>();
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
                if (trigger == Trigger.When_enters_the_battlefield){
                    _abilityQueue.AddAbility(entity, ability);
                    continue;
                }
                
                presentAbilities.Add(ability);

                // Beginning of phase triggers
                var phase = TriggerToPhase(trigger);
                if (phase != Phase.None && !_phaseTriggers.Contains(phase)){
                    print($"New phase trigger: {trigger} in phase {phase}");
                    _phaseTriggers.Add(phase);
                }
            }

            _presentAbilities.Add(entity, presentAbilities);
        }
    }

    public void CheckPhaseTriggers(Phase phase)
    {
        // TurnManager waits for this to be false
        QueueResolving = true;

        // We only search for abilities if the current phase triggers at least one effect
        if (!_phaseTriggers.Contains(phase)){
            QueueResolving = false;
            return;
        }

        // TODO: Should probably rebuild this to not search ALL entities
        // Maybe create a dict for each trigger separately and add entities to it
        // Then get the corresponding effect from the entity
        // Also think about how to handle multiple effects on the same trigger
        // 1-many relations
        var phaseTrigger = PhaseToTrigger(phase);
        foreach (var entity in _presentAbilities.Keys){
            foreach (var ability in _presentAbilities[entity]){
                if (ability.trigger == phaseTrigger) _abilityQueue.AddAbility(entity, ability);
            }
        }

        // TODO: Verify if there is a better solution to sync the animation and _turnManager continuation
        StartCoroutine(StartResolvingQueue());
    }
    
    public IEnumerator StartResolvingQueue()
    {
        QueueResolving = true;
        
        yield return _abilityQueue.Resolve();

        QueueResolving = false;
    }

    #region Helper Functions
    public void EntityDies(BattleZoneEntity entity) => _presentAbilities.Remove(entity);
    public void ClearAbilitiesQueue() => _abilityQueue.ClearQueue();
    
    private Phase TriggerToPhase(Trigger trigger)
    {
        Phase phase = trigger switch
        {
            Trigger.Beginning_Turn => Phase.PhaseSelection,
            Trigger.Beginning_Draw => Phase.Draw,
            Trigger.Beginning_Invent => Phase.Invent,
            Trigger.Beginning_Develop => Phase.Develop,
            Trigger.Beginning_Combat => Phase.Combat,
            Trigger.Beginning_Recruit => Phase.Recruit,
            Trigger.Beginning_Deploy => Phase.Deploy,
            Trigger.Beginning_Prevail => Phase.Prevail,
            Trigger.Beginning_CleanUp => Phase.CleanUp,
            _ => Phase.None
        };

        return phase;
    }

    private Trigger PhaseToTrigger(Phase phase)
    {
        Trigger trigger = phase switch
        {
            Phase.PhaseSelection => Trigger.Beginning_Turn,
            Phase.Draw => Trigger.Beginning_Draw,
            Phase.Invent => Trigger.Beginning_Invent,
            Phase.Develop => Trigger.Beginning_Develop,
            Phase.Combat => Trigger.Beginning_Combat,
            Phase.Recruit => Trigger.Beginning_Recruit,
            Phase.Deploy => Trigger.Beginning_Deploy,
            Phase.Prevail => Trigger.Beginning_Prevail,
            Phase.CleanUp => Trigger.Beginning_CleanUp,
            _ => Trigger.None
        };

        return trigger;
    }
    #endregion
}