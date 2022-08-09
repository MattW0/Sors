using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class ScriptableCard : ScriptableObject
{
    [Header("Image")]
    public Sprite image;

    [Header("Properties")]
    public string hash;
    public bool isCreature;
    public int cost;
    public int attack;
    public int health;
    public string title;
    public string description;

    // We can't pass ScriptableCards over the Network, but we can pass uniqueIDs.
    // Throughout this project, you'll find that I've passed uniqueIDs through the Server,
    static Dictionary<string, ScriptableCard> _cache;
    public static Dictionary<string, ScriptableCard> Cache
    {
        get
        {
            if (_cache == null)
            {
                // Load all ScriptableCards from our Resources folder
                ScriptableCard[] cards = Resources.LoadAll<ScriptableCard>("CreatureCards/");
                _cache = cards.ToDictionary(card => card.hash, card => card);
            }
            return _cache;
        }
    }
}
