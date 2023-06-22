using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardEffectsHandler : NetworkBehaviour
{
    public static CardEffectsHandler Instance { get; private set; }
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private PlayerInterfaceManager _playerInterfaceManager;
    public float effectWaitTime = 0.5f;
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
                StartCoroutine(FindEntityOwner(entity, cardInfo.effects[index], trigger));
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

        var phaseTrigger = PhaseToTrigger(phase);
        foreach (var entity in _entityRelations.Keys){
            var relations = _entityRelations[entity];
            foreach (var trigger in relations.Keys){
                if (trigger != phaseTrigger) continue;
                
                var effect = relations[trigger];
                StartCoroutine(FindEntityOwner(entity, effect, trigger));
            }
        }

        // Only have to call turnManager.NextTurnState if the effect triggers at the beginning of a phase
        // TODO: Verify if there is a better solution to sync the animation and turnManager continuation
        turnManager.NextTurnState(_nextPhase);
    }
    
    private IEnumerator FindEntityOwner(BattleZoneEntity entity, Effects effect, Triggers trigger){
        // Wait for Owner (and other info) to be initialized
        // It isn't if entity ETBd as the last action before this trigger
        while(!entity.Owner) yield return null;

        _playerInterfaceManager.RpcLog($"<color=#1118BA>'{entity.Title}': {trigger} -> {effect}</color>");
        StartCoroutine(EffectAnimation(entity, effect));
    }

    private IEnumerator EffectAnimation(BattleZoneEntity entity, Effects effect){
        entity.RpcEffectHighlight(true);
        yield return new WaitForSeconds(effectWaitTime);

        ExecuteEffect(entity, effect);
        // if(entity.cardType == CardType.Development) entity.Health -= 1;

        yield return new WaitForSeconds(effectWaitTime);
        entity.RpcEffectHighlight(false);
    }

    #region Effects
    private void ExecuteEffect(BattleZoneEntity entity, Effects effect){
        switch (effect.ToString().Split('_')[0]){
            case "card":
                entity.Owner.DrawCards(CardDraw(effect));
                break;
            case "price":
                var (cardType, reduction) = PriceReduction(effect);
                turnManager.PlayerGetsKingdomBonus(entity.Owner, cardType, reduction);
                break;
            case "money":
                entity.Owner.Cash += Money(effect);
                break;
            case "damage":
                if(!entity.Opponent){
                    print("No opponent found");
                    return;
                }
                entity.Opponent.Health -= Damage(effect);
                break;
            case "life":
                entity.Owner.Health += LifeGain(effect);
                break;
        }
    }

    private int CardDraw(Effects effect){
        return effect switch{
            Effects.card_draw_1 => 1,
            Effects.card_draw_2 => 2,
            Effects.card_draw_3 => 3,
            _ => 0
        };
    }

    private (CardType, int) PriceReduction(Effects effect){
        var strArray = effect.ToString().Split('_');
        
        // Card type is always the second last element
        var type = strArray[^2];
        type = type[0].ToString().ToUpper() + type.Substring(1);
        Enum.TryParse(type, out CardType cardType);

        // Reduction is always the last element
        var reduction = int.Parse(strArray[^1], CultureInfo.InvariantCulture);
        
        return (cardType, reduction);
    }

    private int Money(Effects effect){
        return effect switch{
            Effects.money_1 => 1,
            Effects.money_2 => 2,
            Effects.money_3 => 3,
            _ => 0
        };
    }

    private int Damage(Effects effect){
        return effect switch{
            Effects.damage_to_opponent_1 => 1,
            // Effects.damage_2 => 2,
            // Effects.damage_3 => 3,
            _ => 0
        };
    }

    private int LifeGain(Effects effect){
        return effect switch{
            Effects.life_gain_1 => 1,
            // Effects.life_2 => 2,
            // Effects.life_3 => 3,
            _ => 0
        };
    }

    #endregion

    #region Helper Functions
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
    #endregion
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

    price_reduction_creature_1,
    price_reduction_development_1,
    price_reduction_money_1,

    money_1,
    money_2,
    money_3,

    damage_to_opponent_1,
    life_gain_1,
}
