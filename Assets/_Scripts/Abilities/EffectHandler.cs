using System;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EffectHandler : MonoBehaviour
{
    private TurnManager _turnManager;
    private PlayerInterfaceManager _playerInterfaceManager;

    [Header("Source and Target(s)")]
    [SerializeField] private BattleZoneEntity _source;
    // TODO: Make this a list for multiple targets
    [SerializeField] private BattleZoneEntity _target;

    [Header("Effects")]
    [SerializeReference] private Dictionary<Effect, IEffect> EFFECTS = new()
    {
        {Effect.Damage, new Damage()},
        {Effect.MoneyGain, new Money()},
        {Effect.LifeGain, new Life()},
        {Effect.CardDraw, new CardDraw()},
        {Effect.PriceReduction, new PriceReduction()},
        {Effect.Curse, new Curse()},
    };

    public static event Action<BattleZoneEntity, Ability> OnPlayerStartSelectTarget;

    private void Start()
    {
        _turnManager = TurnManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;

        foreach (var effect in EFFECTS.Values) effect.VFXSystem = AbilitiesVFXSystem.Instance;
    }

    internal async UniTask Execute(BattleZoneEntity source, Ability ability)
    {
        print($" - {ability} (executing)");
        _source = source;

        // Evaluate target (some are obvious, some require player input)
        _target = GetTargetEntity(ability);
        await UniTask.Delay(SorsTimings.effectTrigger);
        
        // Wait for player input if needed
        if (_target == null) OnPlayerStartSelectTarget?.Invoke(_source, ability);
        while(_target == null) { await UniTask.Delay(100); }
        
        _playerInterfaceManager.RpcLog(_source.Owner.ID, _source.Title, _target.Title , LogType.AbilityExecution);
        await EFFECTS[ability.effect].Execute(_source, _target, ability.amount);
    }

    internal void SetTarget(BattleZoneEntity target)
    {
        _target = target;
        _source.RpcDeclaredTarget(_target);
    }

    private BattleZoneEntity GetTargetEntity(Ability ability)
    {
        // Some targets are pre-determined
        if(ability.target == Target.None) throw new Exception("Ability " + ability + " has no target!");
        
        // Some targets only have one option
        if(ability.target == Target.Self) return _source;
        if(ability.target == Target.You) return _source.Owner.GetEntity();
        if(ability.target == Target.Opponent) return _turnManager.GetOpponentPlayer(_source.Owner).GetEntity();

        // Some effects always target the owner 
        //TODO: could let opponent benefit from these too
        if (ability.effect == Effect.MoneyGain
            || ability.effect == Effect.CardDraw
            || ability.effect == Effect.PriceReduction)
            return _source.Owner.GetEntity();
        
        return null;
    }
}
