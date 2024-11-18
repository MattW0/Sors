using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct CardInfo : IEquatable<CardInfo>
{
    public int goID;
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
    [TextArea] public string description;
    [TextArea] public string flavourText;

    [Header("Sprites")]
    public string cardSpritePath;
    public string entitySpritePath;

    public CardInfo(ScriptableCard card, int gameObjectID = -1)
    {
        goID = gameObjectID;

        // Base properties
        isStartCard = card.isStartCard;
        type = card.type;
        cost = card.cost;
        hash = card.hash ?? card.resourceName;
        title = card.title ?? card.resourceName;
        
        // Money properties
        moneyValue = card.moneyValue;

        // Enitity properties
        health = card.health;
        points = card.points;

        // Triggers and effects
        abilities = card.abilities;

        // Creature properties
        attack = card.attack;
        traits = card.traits;
        
        // Text fields
        description = card.description;
        flavourText = card.flavourText;

        // Resources 
        resourceName = card.resourceName;
        var path = $"Sprites/Cards/{type}/{resourceName}";
        cardSpritePath = path + "/c";
        entitySpritePath = path + "/e";
    }

    public readonly bool Equals(CardInfo other)
    {
        // if (other == null) return false;
        return goID == other.goID && title == other.title && hash == other.hash;
    }
}
