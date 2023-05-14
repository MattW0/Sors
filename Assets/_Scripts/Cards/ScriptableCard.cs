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

    [Header("Creature properties")]
    public int attack;
    public int health;
    public int points;
    public List<Keywords> keyword_abilities;
    public List<string> relations_texts;

    [Header("Money properties")]
    public bool isCreature;
    public int moneyValue;

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

public enum Triggers
{
    // When NAME
    When_enters_the_battlefield,
    When_attacks,
    When_blocks,
    When_dies,
    When_is_put_into_the_discard_pile,
    When_gets_blocked,

    // Whenever NAME
    Whenever_becomes_a_target,
    Whenever_takes_damage,
    Whenever_deals_damage,
    Whenever_deals_combat_damage,
    Whenever_deals_damage_to_a_player,

    // At the beginning of NO NAME
    Beginning_phase_DrawI,
    Beginning_phase_Develop,
    Beginning_phase_Deploy,
    Beginning_combat,
    Beginning_phase_DrawII,
    Beginning_phase_Recruit,
    Beginning_phase_Prevail,
    Beginning_when_you_gain_the_initiative
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
