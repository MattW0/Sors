using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class AbilityQueue : MonoBehaviour
{
    private BoardManager _boardManager;
    private EffectHandler _effectHandler;
    [SerializeField] private Dictionary<BattleZoneEntity, List<Ability>> _queue = new();

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
        print($" - {ability} ({entity.Title})");
        entity.RpcSetHighlight(true, SorsColors.triggerHighlight);

        if (!_queue.ContainsKey(entity)) _queue.Add(entity, new() { ability });
        else _queue[entity].Add(ability);
    }

    public async UniTask Resolve()
    {
        foreach(var (entity, abilities) in _queue)
        {
            foreach(var ability in abilities)
            {
                if (entity == null){
                    print("Entity has been destroyed, skipping ability");
                    continue;
                }

                // Highlight entity
                entity.RpcSetHighlight(true, SorsColors.abilityHighlight);

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

                // Destroy dead entities and target arrows
                _boardManager.BoardCleanUp();
            }
        }

        _queue.Clear();
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

    public void OnDestroy()
    {
        PlayerManager.OnPlayerChooseEntityTarget += PlayerChoosesAbilityTarget;
    }
}
