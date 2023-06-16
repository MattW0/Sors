using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardEffectsHandler : NetworkBehaviour
{
    public static CardEffectsHandler Instance { get; private set; }
    [SerializeField] private TurnManager turnManager;
    private List<Phase> _phaseTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, Dictionary<Triggers, Effects>> _entityRelations = new();
    private Phase _nextPhase;

    private void Awake() {
        if (!Instance) Instance = this;
    }

    public void CardIsPlayed(PlayerManager owner, BattleZoneEntity entity, CardInfo cardInfo){
        if (cardInfo.triggers.Count == 0) return;

        var relations = new Dictionary<Triggers, Effects>();
        foreach (var trigger in cardInfo.triggers){
            // Triggers and effects have the same index and a 1-1 relation
            var index = cardInfo.triggers.IndexOf(trigger);

            // Dont add the relation to the dict as it resolves immediately
            if (trigger == Triggers.When_enters_the_battlefield){
                print($"{cardInfo.title} triggers on ETB with effect {cardInfo.effects[index]}");
                StartCoroutine(EffectAnimation(owner, entity, cardInfo.effects[index]));
                continue;
            }
            
            relations.Add(trigger, cardInfo.effects[index]);

            // Beginning of phase triggers
            var phase = TriggerToPhase(trigger);
            // print($"New phase trigger: {trigger} in phase {phase}");
            if (phase != Phase.None && !_phaseTriggers.Contains(phase))
                _phaseTriggers.Add(phase);
        }

        _entityRelations.Add(entity, relations);
    }

    public void EntityDies(BattleZoneEntity entity){
        _entityRelations.Remove(entity);
    }

    public void CheckPhaseTriggers(Phase phase){
        // We only search relations if the current phase triggers at least one effect
        _nextPhase = phase;
        if (!_phaseTriggers.Contains(phase)){
            turnManager.NextTurnState(_nextPhase);
            return;
        }

        // TODO: Should probably rebuild this to not search ALL entities
        // Maybe create a dict for each trigger separately and add entities to it
        // Then get the corresponding effect from the entity
        // Also think about how to handle multiple effects on the same trigger
        // 1-many relations

        bool hasTriggered = false;
        var phaseTrigger = PhaseToTrigger(phase);
        foreach (var entity in _entityRelations.Keys){
            var relations = _entityRelations[entity];
            foreach (var trigger in relations.Keys){
                if (trigger != phaseTrigger) continue;
                
                hasTriggered = true;
                var effect = relations[trigger];
                print($"{entity.Title} triggers at phase {phase} with effect {relations[trigger]}");
                StartCoroutine(EffectAnimation(entity, effect));
            }
        }

        if (!hasTriggered) turnManager.NextTurnState(_nextPhase);
    }
    
    private IEnumerator EffectAnimation(BattleZoneEntity entity, Effects effect){
        var owner = entity.Owner;

        // TODO: Owner is null if entity ETBd as the last action before this method call
        // network timing issues... similar to skipping phases after combat auto-skip ?
        if(!owner) {
            print("Owner is null! Skipping effect... ");
            turnManager.NextTurnState(_nextPhase);
        } else {
            StartCoroutine(EffectAnimation(owner, entity, effect));
        }

        yield return null;
    }

    // Overload because owner is not set on entity as it ETBs
    private IEnumerator EffectAnimation(PlayerManager owner, BattleZoneEntity entity, Effects effect){
        entity.RpcEffectHighlight(true);
        yield return new WaitForSeconds(1f);
        
        var nbCards = CardDraw(effect);
        owner.DrawCards(nbCards);
        yield return new WaitForSeconds(1f);
        entity.RpcEffectHighlight(false);

        turnManager.NextTurnState(_nextPhase);
        yield return null;
    }

    private int CardDraw(Effects effect){
        return effect switch
        {
            Effects.card_draw_1 => 1,
            Effects.card_draw_2 => 2,
            Effects.card_draw_3 => 3,
            _ => 0
        };
    }

    private Phase TriggerToPhase(Triggers trigger){
        Phase phase = trigger switch
        {
            Triggers.Beginning_Draw => Phase.Draw,
            Triggers.Beginning_Invent => Phase.Invent,
            Triggers.Beginning_Develop => Phase.Develop,
            Triggers.Beginning_Combat => Phase.Combat,
            Triggers.Beginning_Recruit => Phase.Recruit,
            Triggers.Beginning_Deploy => Phase.Deploy,
            Triggers.Beginning_Prevail => Phase.Prevail,
            _ => Phase.None
        };

        return phase;
    }

    private Triggers PhaseToTrigger(Phase phase){
        Triggers trigger = phase switch
        {
            Phase.Draw => Triggers.Beginning_Draw,
            Phase.Invent => Triggers.Beginning_Invent,
            Phase.Develop => Triggers.Beginning_Develop,
            Phase.Combat => Triggers.Beginning_Combat,
            Phase.Recruit => Triggers.Beginning_Recruit,
            Phase.Deploy => Triggers.Beginning_Deploy,
            Phase.Prevail => Triggers.Beginning_Prevail,
            _ => Triggers.None
        };

        return trigger;
    }
}

public enum Triggers
{
    None,
    // When NAME
    When_enters_the_battlefield,
    // When_attacks,
    // When_blocks,
    When_dies,
    // When_is_put_into_the_discard_pile,
    // When_gets_blocked,

    // Whenever NAME
    // Whenever_becomes_a_target,
    // Whenever_takes_damage,
    // Whenever_deals_damage,
    // Whenever_deals_combat_damage,
    // Whenever_deals_damage_to_a_player,

    // At the beginning of [PHASE]
    Beginning_Turn,
    Beginning_Draw,
    Beginning_Invent,
    Beginning_Develop,
    Beginning_Combat,
    Beginning_Recruit,
    Beginning_Deploy,
    Beginning_Prevail,
    // Beginning_when_you_gain_the_initiative
}

public enum Effects
{
    card_draw_1,
    card_draw_2,
    card_draw_3,

    creature_price_reduction_1,
    development_price_reduction_1,

    money_1,
    money_2,
    money_3,

    damage_to_opponent_1,
    life_gain_1,
}
