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
    public int cost;
    public string title;
    public string description;
    // public Sprite image;

    [Header("Creature properties")]
    public int attack;
    public int health;

    [Header("Money properties")]
    public bool isCreature;
    public int moneyValue;

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
