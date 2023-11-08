using UnityEngine.EventSystems;

public class AttackerArrowHandler : ArrowHandler, IPointerClickHandler
{
    private BattleZoneEntity _entity;
    private CreatureEntity _creature;

    private void Awake()
    {
        _entity = gameObject.GetComponent<BattleZoneEntity>();
        _creature = gameObject.GetComponent<CreatureEntity>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // return if not in Attackers Phase
        if (CurrentCombatState != CombatState.Attackers) return;

        if (_entity.isOwned) HandleClickedMyEntity();
        else HandleClickedOpponentEntity();
    }

    private void HandleClickedMyEntity()
    {
        if (!_creature.CanAct || HasTarget) return;

        if (!HasOrigin)
        {
            SpawnArrow();
            _creature.IsAttacking = true;
            _entity.Owner.PlayerChoosesAttacker(_creature);
        }
        else
        {
            RemoveArrow(true);
            _creature.IsAttacking = false;
            _entity.Owner.PlayerRemovesAttacker(_creature);
        }
    }

    private void HandleClickedOpponentEntity()
    {
        if (!_entity.IsTargetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!clicker.PlayerIsChoosingAttack) return;

        clicker.PlayerChoosesTargetToAttack(_entity);
    }

    private void OnDestroy()
    {
    }
}
