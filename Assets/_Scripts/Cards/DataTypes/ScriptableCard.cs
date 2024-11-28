using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class ScriptableCard : ScriptableObject
{
    [Header("Configuration Properties")]
    public bool isStartCard;
    public string hash;
    public string resourceName;
    
    [Header("Base properties")]
    public CardType type;
    public string title;
    public int cost;
    public int health;
    public int attack;
    public int points;
    public int moneyValue;

    [Header("Abilities")]
    public List<Ability> abilities;

    [Header("Creature properties")]
    public List<Traits> traits;

    [Header("Display Texts")]
    public string flavourText;
    [TextArea] public string description;
}