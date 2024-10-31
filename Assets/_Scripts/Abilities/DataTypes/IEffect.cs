using System.Collections;
using UnityEngine;

public interface IEffect
{
    public void Init(AbilitiesVFXSystem abilitiesVFXSystem, WaitForSeconds wait);
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount);
}
