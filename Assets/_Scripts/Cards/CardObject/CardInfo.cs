using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[System.Serializable]
public struct CardInfo : IEquatable<CardInfo>
{
    public int goID;
    public bool isStartCard;
    public string hash;
    public string resourceName;
    [Header("General stats")]
    public CardType type;
    public string title;
    public string description;
    public int cost;
    public int health;
    public int attack;
    public int points;
    public int moneyValue;

    [Header("Triggers and effects")]
    public List<Ability> abilities;

    [Header("Creature properties")]
    public List<Keywords> keywordAbilities;

    [Header("Sprites")]
    public string cardSpritePath;
    public string entitySpritePath;

    public CardInfo(ScriptableCard card, int gameObjectID = -1)
    {
        goID = gameObjectID;

        isStartCard = card.isStartCard;
        hash = card.hash;
        resourceName = card.resourceName;
        type = card.type;
        title = card.title;
        cardSpritePath = $"Sprites/Cards/{type.ToString()}/{resourceName}/c";
        entitySpritePath = $"Sprites/Cards/{type.ToString()}/{resourceName}/e";

        description = card.description;
        cost = card.cost;

        // Money properties
        moneyValue = card.moneyValue;

        // Enitity properties
        health = card.health;
        points = card.points;

        // Triggers and effects
        abilities = card.abilities;

        // Creature properties
        attack = card.attack;
        keywordAbilities = card.keywordAbilities;
    }

    public void Destroy(){
        this.hash = null;
        this.title = null;
        this.cost = 0;
        // this.image = null;

        this.attack = 0;
        this.health = 0;
        this.goID = 0;

        // this.type = CardType.Technology;
        this.moneyValue = 0;

        this.keywordAbilities = null;
    }

    public bool Equals(CardInfo other)
    {
        // if (other == null) return false;
        return goID == other.goID && title == other.title && hash == other.hash;
    }
}

public enum CardType : byte
{
    Creature,
    Money,
    Technology,
    Player,
    None
}
