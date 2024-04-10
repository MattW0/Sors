using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityQueue : MonoBehaviour
{
    public static AbilityQueue Instance { get; private set; }
    private TurnManager _turnManager;
    private BoardManager _boardManager;
    private PlayerInterfaceManager _playerInterfaceManager;
    private Dictionary<BattleZoneEntity, Ability> _queue = new();
    private bool _continue = false;
    private bool _abilityResolving = false;
    private BattleZoneEntity _abilitySource;
    // TODO: Make this a list for multiple targets
    private BattleZoneEntity _abilityTarget;

    private void Awake() {
        if (!Instance) Instance = this;
    }

    public void Start()
    {
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    // TODO : Need function to handle triggered Abilities like taking damage, gain health etc.
    // Or should this be on entity and then added to queue from externally ?
    public void AddAbility(BattleZoneEntity entity, Ability ability)
    {
        _playerInterfaceManager.RpcLog($"'{entity.Title}': {ability.trigger} -> {ability.effect}", LogType.EffectTrigger);
        _queue.Add(entity, ability);
    }

    public IEnumerator Resolve()
    {
        foreach(var (entity, ability) in _queue){
            _abilityResolving = true;
            
            // Waits for player input (with _continue) and when done sets _abilityResolving = true
            yield return ResolveAbility(entity, ability);

            _abilitySource = null;
            _abilityTarget = null;
        }
        _queue.Clear();
    }

    private IEnumerator ResolveAbility(BattleZoneEntity entity, Ability ability)
    {
        print($"Resolving {entity.Title} ability : " + ability.ToString());
        
        // 1) Wait for player input if needed
        entity.RpcEffectHighlight(true);
        yield return new WaitForSeconds(SorsTimings.effectTrigger);

        _continue = CanContinueWithoutPlayerInput(entity, ability);
        while(!_continue) {
            yield return new WaitForSeconds(0.1f);
        }
        _continue = false;

        // 2) Effect animation
        StartCoroutine(ExecuteEffect(entity, ability));
        while(!_continue) {
            yield return new WaitForSeconds(0.1f);
        }
        _continue = false;

        yield return new WaitForSeconds(SorsTimings.effectExecution);
        _abilityResolving = false;
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
        if(!(ability.effect == Effect.Damage || ability.effect == Effect.LifeGain)){
            return true;
        }

        // Else we need input from player and set _continue to true after receiving it
        print($"NEED PLAYER INPUT: " + ability.ToString());
        _abilitySource = entity;
        _boardManager.PlayerStartSelectTarget(entity, ability);
        
        return false;
    }

    public void PlayerChoosesAbilityTarget(BattleZoneEntity target){
        _boardManager.ResetTargeting();

        _abilityTarget = target;
        _abilitySource.RpcDeclaredTarget(_abilityTarget);

        // continue in ResolveAbility (Step 2)
        _continue = true;
    }

    #region Effects
    private IEnumerator ExecuteEffect(BattleZoneEntity entity, Ability ability)
    {
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
    }

    private void HandleCardDraw(BattleZoneEntity entity, Ability ability){
        entity.Owner.DrawCards(ability.amount);
    }

    private void HandlePriceReduction(BattleZoneEntity entity, Ability ability)
    {
        // convert ability.target (EffectTarget enum) to CardType enum
        var cardType = (CardType)Enum.Parse(typeof(CardType), ability.target.ToString());
        var reduction = ability.amount;

        _turnManager.PlayerGetsMarketBonus(entity.Owner, cardType, reduction);
    }

    private void HandleMoneyGain(BattleZoneEntity entity, Ability ability){
        entity.Owner.Cash += ability.amount;
    }

    private void HandleDamage(BattleZoneEntity entity, Ability ability){       
        // With target
        if(_abilityTarget) {
            // TODO: This needs clean-up together with how player and entity health are linked
            _abilityTarget.EntityTakesDamage(ability.amount, entity.CardInfo.keywordAbilities.Contains(Keywords.Deathtouch));
            return;
        }

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

    }

    private void HandleLifeGain(BattleZoneEntity entity, Ability ability){
        entity.Owner.Health += ability.amount;
    }
    #endregion

    public void ClearQueue(){
        _queue.Clear();
    }
}
