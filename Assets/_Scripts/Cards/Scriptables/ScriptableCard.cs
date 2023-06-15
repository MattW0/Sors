using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class ScriptableCard : ScriptableObject
{
    [Header("General Properties")]
    public string hash;
    public CardType type;
    public int cost;
    public string title;

    [Header("Enitity properties")]
    public int health;
    public int attack;
    public int points;

    [Header("Money properties")]
    public int moneyValue;

    [Header("Special Effects")]
    public List<Triggers> triggers;
    public List<Effects> effects;

    [Header("Creature properties")]
    public List<Keywords> keywordAbilities;
    public List<string> relationsTexts;

    [Header("Card Display Data")]
    public string description;
    public Sprite image;

    static Dictionary<string, ScriptableCard> _cache;
    public static Dictionary<string, ScriptableCard> Cache
    {
        get {
            if (_cache == null) {
                // Load all ScriptableCards from our Resources folder
                Debug.Log("Caching cards");
                ScriptableCard[] cards = Resources.LoadAll<ScriptableCard>("CreatureCards/");
                _cache = cards.ToDictionary(card => card.hash, card => card);
            }
            return _cache;
        }
    }
}

// May need these in the future to trigger during the game
public enum Keywords{
    Deathtouch,
    Defender,
    Attacker,
    Double_Strike,
    First_Strike,
    Flying,
    Haste,
    Hexproof,
    Indestructible,
    Lifelink,
    Menace,
    Protection,
    Reach,
    Shroud,
    Trample,
    Vigilance,
}

public enum SpecialAbilities{
    // With X -> X needs to be differently distributed probably:
    gain_X_life,
    mill_X,
    scry_X,

    // With X and special
    Put_X_Marker_marker_on_target_creature,
    create_X_treasure_token,
    create_X_creature_token,

    // With target
    deal_X_damage_to_target_creature,
    destroy_target_creature,
    exile_target_creature,
}
