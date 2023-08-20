using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardEffectsHandler : NetworkBehaviour
{
    public static CardEffectsHandler Instance { get; private set; }
    private TurnManager _turnManager;
    private PlayerInterfaceManager _playerInterfaceManager;
    public float effectWaitTime = 0.5f;
    private bool _continue = false;
    public bool Continue { 
        get => _continue; 
        set {
            _continue = value;
            // if(value) _turnManager.NextTurnState(_nextPhase);
        }
    }

    // Helper fields
    private Phase _nextPhase;
    private List<Phase> _phaseTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, List<Ability>> _presentAbilities = new();

    private void Awake() {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _turnManager = TurnManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    public void CardIsPlayed(PlayerManager owner, BattleZoneEntity entity, CardInfo cardInfo){
        if (cardInfo.abilities.Count == 0) return;

        var activeAbilities = new List<Ability>();
        foreach (var ability in cardInfo.abilities){
            // Triggers and effects have the same index and a 1-1 relation
            var trigger = ability.trigger;

            // Dont add the relation to the dict as it resolves immediately
            if (trigger == Trigger.When_enters_the_battlefield){
                StartCoroutine(BeginAbility(entity, ability));
                continue;
            }
            
            activeAbilities.Add(ability);

            // Beginning of phase triggers
            var phase = TriggerToPhase(trigger);
            // print($"New phase trigger: {trigger} in phase {phase}");
            if (phase != Phase.None && !_phaseTriggers.Contains(phase))
                _phaseTriggers.Add(phase);
        }

        _presentAbilities.Add(entity, activeAbilities);
    }

    public void EntityDies(BattleZoneEntity entity){
        _presentAbilities.Remove(entity);
    }

    public void CheckPhaseTriggers(Phase phase){
        // We only search relations if the current phase triggers at least one effect
        _nextPhase = phase;
        if (!_phaseTriggers.Contains(phase)){
            _turnManager.NextTurnState(_nextPhase);
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
                if (ability.trigger != phaseTrigger) 
                    continue;
                StartCoroutine(BeginAbility(entity, ability));
            }
        }

        // Only have to call _turnManager.NextTurnState if the effect triggers at the beginning of a phase
        // TODO: Verify if there is a better solution to sync the animation and _turnManager continuation
    }
    
    private IEnumerator BeginAbility(BattleZoneEntity entity, Ability ability){
        // Wait for Owner (and other info) to be initialized
        // It isn't if entity ETBd as the last action before this trigger
        while(!entity.Owner) yield return null;

        _playerInterfaceManager.RpcLog($"'{entity.Title}': {ability.trigger} -> {ability.effect}", LogType.EffectTrigger);
        print($"'{entity.Title}': {ability.trigger} -> {ability.effect}");
        StartCoroutine(BeginAbilityExecution(entity, ability));
    }

    private IEnumerator BeginAbilityExecution(BattleZoneEntity entity, Ability ability){

        entity.RpcEffectHighlight(true);
        CheckForPlayerInput(entity, ability);
        yield return new WaitForSeconds(effectWaitTime);

        while(!_continue) {
            // print("Waiting for player input");
            yield return new WaitForSeconds(effectWaitTime);
        }
        print("Player input received");
        _continue = false;

        ExecuteEffect(entity, ability);
        
        
        // if(entity.cardType == CardType.Technology) entity.Health -= 1;

        yield return new WaitForSeconds(effectWaitTime);
        entity.RpcEffectHighlight(false);
    }

    private void CheckForPlayerInput(BattleZoneEntity entity, Ability ability){
        
        print($"Ability: {ability.effect} - {ability.target} - {ability.amount} - {ability.trigger}");
        print("Continue: " + _continue);
        // Only need input for damage and lifegain currently
        if(!(ability.effect.Equals(Effect.Damage) || ability.effect.Equals(Effect.LifeGain))){
            _continue = true;
            return;
        }

        _turnManager.PlayerStartSelectTarget(entity, ability);
        if (ability.target == EffectTarget.AnyPlayer){

        } else if (ability.target == EffectTarget.Entity){
        } else if (ability.target == EffectTarget.Technology){

        } else if (ability.target == EffectTarget.Creature){

        } else if (ability.target == EffectTarget.Any){

        }
    }

    #region Effects
    private void ExecuteEffect(BattleZoneEntity entity, Ability ability){
        var effect = ability.effect;

        switch (effect){
            case Effect.CardDraw:
                HandleCardDraw(entity, ability);
                break;
            case Effect.PriceReduction:
                HandlePriceReduction(entity, ability);
                break;
            case Effect.MoneyGain:
                HandleMoneyGain(entity, ability);
                break;
            case Effect.Damage:
                HandleDamage(false, entity, ability);
                break;
            case Effect.LifeGain:
                HandleLifeGain(entity, ability);
                break;
        }
    }

    private void HandleCardDraw(BattleZoneEntity entity, Ability ability){
        entity.Owner.DrawCards(ability.amount);
    }

    private void HandlePriceReduction(BattleZoneEntity entity, Ability ability){
        
        // convert ability.target (EffectTarget enum) to CardType enum
        var cardType = (CardType)Enum.Parse(typeof(CardType), ability.target.ToString());
        var reduction = ability.amount;

        _turnManager.PlayerGetsKingdomBonus(entity.Owner, cardType, reduction);
    }

    private void HandleMoneyGain(BattleZoneEntity entity, Ability ability){
        entity.Owner.Cash += ability.amount;
    }

    private void HandleDamage(bool needsInput, BattleZoneEntity entity, Ability ability){       
        // Without player input
        if(ability.target == EffectTarget.Opponent){
            if(!entity.Opponent){ // For single player
                print("No opponent found");
            } else {
                entity.Opponent.Health -= ability.amount;
            }
        } else if (ability.target == EffectTarget.Player){
            entity.Owner.Health -= ability.amount;
        } else if (ability.target == EffectTarget.Self){
            entity.Health -= ability.amount;
        }
    }

    private void HandleLifeGain(BattleZoneEntity entity, Ability ability){
        entity.Owner.Health += ability.amount;
    }

    #endregion

    #region Helper Functions
    private Phase TriggerToPhase(Trigger trigger){
        Phase phase = trigger switch
        {
            Trigger.Beginning_Draw => Phase.Draw,
            Trigger.Beginning_Invent => Phase.Invent,
            Trigger.Beginning_Develop => Phase.Develop,
            Trigger.Beginning_Combat => Phase.Combat,
            Trigger.Beginning_Recruit => Phase.Recruit,
            Trigger.Beginning_Deploy => Phase.Deploy,
            Trigger.Beginning_Prevail => Phase.Prevail,
            _ => Phase.None
        };

        return phase;
    }

    private Trigger PhaseToTrigger(Phase phase){
        Trigger trigger = phase switch
        {
            Phase.Draw => Trigger.Beginning_Draw,
            Phase.Invent => Trigger.Beginning_Invent,
            Phase.Develop => Trigger.Beginning_Develop,
            Phase.Combat => Trigger.Beginning_Combat,
            Phase.Recruit => Trigger.Beginning_Recruit,
            Phase.Deploy => Trigger.Beginning_Deploy,
            Phase.Prevail => Trigger.Beginning_Prevail,
            _ => Trigger.None
        };

        return trigger;
    }
    #endregion
}