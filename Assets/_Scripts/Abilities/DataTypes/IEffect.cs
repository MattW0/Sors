using System.Collections;
using UnityEngine;

public interface IEffect
{
    public AbilitiesVFXSystem VFXSystem { get; set; }

    // Have to use BattleZoneEntity types in all VFX methods too because positions are different on clients
    public IEnumerator Execute(BattleZoneEntity source, BattleZoneEntity target, int amount);
}
