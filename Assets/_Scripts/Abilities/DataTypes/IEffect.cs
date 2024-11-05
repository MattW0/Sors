using System.Collections;
using UnityEngine;

public interface IEffect
{
    public void Init(AbilitiesVFXSystem abilitiesVFXSystem, WaitForSeconds wait);

    // Have to use BattleZoneEntity types in all VFX methods too because positions
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount);
}
