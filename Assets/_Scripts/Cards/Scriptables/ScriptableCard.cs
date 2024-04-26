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
    public List<Traits> traits;
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