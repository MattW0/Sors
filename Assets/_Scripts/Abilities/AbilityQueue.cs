using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class AbilityQueue : MonoBehaviour
{
    private BoardManager _boardManager;
    private EffectHandler _effectHandler;
    [SerializeField] private AbilityQueueUI _ui;
    [SerializeField] private Queue<(BattleZoneEntity, Ability)> _queue = new();

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
        print($" - Adding ability {ability} ({entity.Title})");
        entity.RpcSetHighlight(true, SorsColors.triggerHighlight);

        _queue.Enqueue((entity, ability));
        _ui.RpcAddAbility(entity, ability);
    }

    public async UniTask Resolve()
    {
        // Destroy dead entities and target arrows, at least once because is inbetween transitions
        await _boardManager.BoardCleanUp();

        while(_queue.Count > 0)
        {
            var (entity, ability) = _queue.Dequeue();
        
            if (entity == null){
                print("Entity has been destroyed, skipping ability");
                continue;
            }

            // Highlight entity
            entity.RpcSetHighlight(true, SorsColors.abilityHighlight);
            _ui.RpcStartNextAbility();

            // No valid target on board -> continue
            if (! _boardManager.PlayerHasValidTarget(ability)){
                entity.RpcSetHighlight(false, SorsColors.defaultHighlight);
                continue;
            }

            // Execute effect
            await _effectHandler.Execute(entity, ability);
            entity.RpcSetHighlight(false, SorsColors.defaultHighlight);

            // Wait some more to prevent too early clean-up (destroying dead entities)
            await UniTask.Delay(100);
            _ui.RpcRemoveAbility();

            // Destroy dead entities and target arrows
            await _boardManager.BoardCleanUp();
        }
        
        _queue.Clear();
        _ui.WindowOut();
    }

    public void PlayerChoosesAbilityTarget(BattleZoneEntity target)
    {
        _boardManager.ResetTargeting();
        _effectHandler.SetTarget(target);
    }

    public void ClearQueue()
    {
        _queue.Clear();
    }

    public int GetQueueCount()
    {
        return _queue.Count;
    } 

    public void OnDestroy()
    {
        PlayerManager.OnPlayerChooseEntityTarget += PlayerChoosesAbilityTarget;
    }
}
