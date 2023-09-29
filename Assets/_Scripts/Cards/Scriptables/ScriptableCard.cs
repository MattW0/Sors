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
    public List<Ability> abilities;

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