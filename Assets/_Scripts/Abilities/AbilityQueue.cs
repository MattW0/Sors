using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class AbilityQueue : MonoBehaviour
{
    private BoardManager _boardManager;
    private EffectHandler _effectHandler;
    [SerializeField] private Dictionary<BattleZoneEntity, Ability> _queue = new();
    private bool _continue = false;

    private void Awake() 
    {
        PlayerManager.OnPlayerChooseEntityTarget += PlayerChoosesAbilityTarget;
    }

    public void Start()
    {
        _boardManager = BoardManager.Instance;
        _effectHandler = GetComponent<EffectHandler>();
    }

    // TODO : Need function to handle triggered Abilities like taking damage, gain health etc.
    // Or should this be on entity and then added to queue from externally ?
    public void AddAbility(BattleZoneEntity entity, Ability ability)
    {
        print($"'{entity.Title}' triggers: {ability}");
        entity.RpcSetHighlight(true, SorsColors.triggerHighlight);
        _queue.Add(entity, ability);
    }

    public async UniTask Resolve()
    {
        foreach(var (entity, ability) in _queue)
        {
            _effectHandler.SetSource(entity, ability);

            if (entity == null){
                print("Entity has been destroyed, skipping ability " + ability.ToString());
                continue;
            }
            entity.RpcSetHighlight(true, SorsColors.abilityHighlight);

            // May need to wait for player to declare target -> set _abilityTarget
            await EvaluateAbilityTarget(entity, ability);

            // Execute effect
            await _effectHandler.Execute();
            entity.RpcSetHighlight(false, SorsColors.defaultHighlight);

            // Wait some more to prevent too early clean-up (destroying dead entities)
            await UniTask.Delay(100);

            // Destroy dead entities and target arrows
            _boardManager.BoardCleanUp();
            _effectHandler.Reset();
        }

        _queue.Clear();
    }

    private async UniTask EvaluateAbilityTarget(BattleZoneEntity entity, Ability ability)
    {
        print($"Evaluate target for {entity.Title} ability : " + ability.ToString());
        
        await UniTask.Delay(SorsTimings.effectTrigger);

        // Wait for player input if needed
        _continue = CanContinueWithoutPlayerInput(entity, ability);
        while(!_continue) {
            await UniTask.Delay(100);
        }
        _continue = false;
    }

    private bool CanContinueWithoutPlayerInput(BattleZoneEntity entity, Ability ability)
    {
        // No target -> continue immediately
        if(ability.target == Target.None){
            return true;
        }

        // Some targets are pre-determined
        if(ability.target == Target.Self
           || ability.target == Target.You
           || ability.target == Target.Opponent)
        {
            return true;
        }

        // Only need input for damage and lifegain currently
        if(ability.effect != Effect.Damage 
           && ability.effect != Effect.LifeGain){
            return true;
        }

        // No valid target on board -> continue immediately
        if (! _boardManager.PlayerHasValidTarget(ability)) return true;
        
        // Else we need input from player and set _continue to true after receiving it
        print($"NEED PLAYER INPUT: " + ability.ToString());
        _boardManager.PlayerStartSelectTarget(entity, ability);
        return false;
    }

    public void PlayerChoosesAbilityTarget(BattleZoneEntity target)
    {
        _boardManager.ResetTargeting();
        _effectHandler.SetTarget(target);

        // continue with ability resolution
        _continue = true;
    }

    public void ClearQueue()
    {
        _queue.Clear();
    }

    public void OnDestroy()
    {
        PlayerManager.OnPlayerChooseEntityTarget += PlayerChoosesAbilityTarget;
    }
}
