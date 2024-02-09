using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class ScriptableCard : ScriptableObject
{
    [Header("General Properties")]
    public bool isStartCard;
    public string hash;
    public string resourceName;
    
    [Header("Card properties")]
    public CardType type;
    public string title;
    public int cost;
    public int health;
    public int attack;
    public int points;

    [Header("Money properties")]
    public int moneyValue;

    [Header("Abilities")]
    public List<Ability> abilities;

    [Header("Creature properties")]
    public List<Keywords> keywordAbilities;
    public List<string> relationsTexts;

    [Header("Card Display Data")]
    public string description;

    static Dictionary<string, ScriptableCard> _cache;
    public static Dictionary<string, ScriptableCard> Cache
    {
        get {
            if (_cache == null) {
                // Load all ScriptableCards from our Resources folder
                Debug.Log("Caching cards");
                ScriptableCard[] cards = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
                _cache = cards.ToDictionary(card => card.hash, card => card);
            }
            return _cache;
        }
    }
}

// May need these in the future to trigger during the game
public enum Keywords
{
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