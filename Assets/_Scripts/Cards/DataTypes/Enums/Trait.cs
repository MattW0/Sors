using System.Collections.Generic;
using UnityEngine;

public enum Trait
{
    None = 0,
    Deathtouch = 1,
    Lifelink = 2,
    Trample = 3,
    Defensive = 4,
    Offensive = 5,
    Menace,
    Flying,
    Reach,
    // FirstStrike,
    // DoubleStrike,
    // Hexproof,
    // Shroud,
}

public static class TraitExtensions
{
    private static readonly Dictionary<Trait, string> Descriptions = new()
    {
        { Trait.Trample, "Excess damage carries over to the initial target." },
        { Trait.Deathtouch, "Any damage dealt by this destroys an entity." },
        { Trait.Lifelink,  "Damage dealt by this creature also heals its controller." },
        { Trait.Defensive, "This entity can only block." },
        { Trait.Offensive, "This creature can only attack." }
    };

    public static string GetDescription(this Trait trait)
    {
        return Descriptions.TryGetValue(trait, out var desc) ? desc : "No description available.";
    }

    public static Sprite GetSprite(this Trait trait)
    {
        // Debug.Log("Sprite path: " + "Sprites/UI/Icons/Traits/" + (int) trait);
        return Resources.Load<Sprite>("Sprites/UI/Icons/Traits/" + (int) trait);
    }
}
