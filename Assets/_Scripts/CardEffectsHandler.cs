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
    private BoardManager _boardManager;
    private PlayerInterfaceManager _playerInterfaceManager;
    private Dictionary<BattleZoneEntity, Ability> _abilityQueue = new();
    public bool QueueResolving { get; private set; }
    private bool _abilityResolving = false;
    private bool _continue = false;
    private BattleZoneEntity _abilitySource;
    // TODO: Make this a list for multiple targets
    private BattleZoneEntity _abilityTarget;

    // Helper fields
    private List<Phase> _phaseTriggers = new(); // List to reduce number of searches on all entities and their triggers
    private Dictionary<BattleZoneEntity, List<Ability>> _presentAbilities = new();

    private void Awake() {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    public IEnumerator CardsArePlayed(List<BattleZoneEntity> entities){
        
        yield return new WaitForSeconds(0.1f);
        foreach (var entity in entities){

            // print($"Entity is played : {entity.Title}");

            List<Ability> abilities = entity.CardInfo.abilities;
            if (abilities == null || abilities.Count == 0) continue;

            var activeAbilities = new List<Ability>();
            foreach (var ability in abilities){
                // Triggers and effects have the same index and a 1-1 relation
                var trigger = ability.trigger;

                // Dont add the relation to the dict as it resolves immediately
                if (trigger == Trigger.When_enters_the_battlefield){
                    AddAbilityToQueue(entity, ability);
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
    }

    public void CheckPhaseTriggers(Phase phase)
    {
        // TurnManager waits for this to be false
        QueueResolving = true;

        // print("Current phases that trigger");
        // foreach(var p in _phaseTriggers)
        //     print(p);

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
                if (ability.trigger == phaseTrigger) AddAbilityToQueue(entity, ability);
            }
        }

        // Only have to call _turnManager.NextTurnState if the effect triggers at the beginning of a phase
        // TODO: Verify if there is a better solution to sync the animation and _turnManager continuation
        StartCoroutine(StartResolvingQueue());
    }
    

    // TODO : Need function to handle triggered Abilities like taking damage, gain health etc.
    // Or should this be on entity and then added to queue from externally ?

    private void AddAbilityToQueue(BattleZoneEntity entity, Ability ability){
        _playerInterfaceManager.RpcLog($"'{entity.Title}': {ability.trigger} -> {ability.effect}", LogType.EffectTrigger);
        print($"'{entity.Title}': {ability.trigger} -> {ability.effect}");
        _abilityQueue.Add(entity, ability);
    }

    public IEnumerator StartResolvingQueue()
    {
        QueueResolving = true;
        // print(" >>> Start resolving ability queue <<< ");
        foreach(var (entity, ability) in _abilityQueue){
            _abilityResolving = true;
            
            // Waits for player input (with _continue) and when done sets _abilityResolving = true
            StartCoroutine(ResolveAbility(entity, ability));
            while(_abilityResolving) {
                yield return new WaitForSeconds(0.1f);
            }

            _abilitySource = null;
            _abilityTarget = null;
        }

        // print(" --- Ability queue resolved --- ");
        QueueResolving = false;
        _abilityQueue.Clear();
    }

    private IEnumerator ResolveAbility(BattleZoneEntity entity, Ability ability)
    {
        print($"Resolving ability : " + ability.ToString());
        
        entity.RpcEffectHighlight(true);
        yield return new WaitForSeconds(SorsTimings.effectTrigger);

        _continue = CanContinueWithoutPlayerInput(entity, ability);
        while(!_continue) {
            yield return new WaitForSeconds(0.1f);
        }
        _continue = false;

        StartCoroutine(ExecuteEffect(entity, ability));
        while(!_continue) {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(SorsTimings.effectExecution);
        _continue = false;
        _abilityResolving = false;
        _boardManager.ResetTargeting();
    }

    private bool CanContinueWithoutPlayerInput(BattleZoneEntity entity, Ability ability)
    {
        // No target -> continue immediately
        if(ability.target == EffectTarget.None){
            return true;
        }

        // Some targets are pre-determined
        if(ability.target == EffectTarget.Self
           || ability.target == EffectTarget.Player
           || ability.target == EffectTarget.Opponent)
        {
            return true;
        }

        // Only need input for damage and lifegain currently
        if(!(ability.effect.Equals(Effect.Damage) || ability.effect.Equals(Effect.LifeGain))){
            return true;
        }

        // Else we need input from player and set _continue to true after receiving it
        print($"NEED PLAYER INPUT: " + ability.ToString());
        _abilitySource = entity;
        _boardManager.PlayerStartSelectTarget(entity, ability);
        
        return false;
    }

    public void PlayerChoosesTargetEntity(BattleZoneEntity target){
        _abilityTarget = target;
        _abilitySource.RpcDeclaredTarget(_abilityTarget);

        // continue in ResolveAbility (Step 2)
        _continue = true;
    }

    public void EntityDies(BattleZoneEntity entity){
        _presentAbilities.Remove(entity);
    }

    #region Effects
    private IEnumerator ExecuteEffect(BattleZoneEntity entity, Ability ability){

        switch (ability.effect){
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
                HandleDamage(entity, ability);
                break;
            case Effect.LifeGain:
                HandleLifeGain(entity, ability);
                break;
        }

        yield return new WaitForSeconds(SorsTimings.effectExecution);
        _continue = true;
        entity.RpcEffectHighlight(false);

        yield return null;
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

    private void HandleDamage(BattleZoneEntity entity, Ability ability){       
        // Without target
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

        // With target
        if(_abilityTarget) _abilityTarget.Health -= ability.amount;
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

    public void ClearAbilitiesQueue()
    {
        _abilityQueue.Clear();
    }
    #endregion
}