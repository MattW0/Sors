using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct CardInfo : IEquatable<CardInfo>
{
    public string goID;
    public string hash;
    public CardType type;
    public string title;
    public int cost;

    [Header("Money properties")]
    public int moneyValue;

    [Header("Creature properties")]
    public int attack;
    public int health;
    public int points;
    public List<Keywords> keyword_abilities;

    public CardInfo(ScriptableCard card, string gameObjectID = null)
    {
        if (gameObjectID != null) goID = gameObjectID;
        else goID = null;

        hash = card.hash;
        type = card.type;
        title = card.title;
        cost = card.cost;

        // Money properties
        moneyValue = card.moneyValue;

        // Creature properties
        attack = card.attack;
        health = card.health;
        points = card.points;
        keyword_abilities = card.keyword_abilities;
    }

    public void Destroy(){
        this.hash = null;
        this.title = null;
        this.cost = 0;
        // this.image = null;

        this.attack = 0;
        this.health = 0;
        this.goID = null;

        // this.type = CardType.Development;
        this.moneyValue = 0;

        this.keyword_abilities = null;
    }

    public bool Equals(CardInfo other)
    {
        // if (other == null) return false;
        return goID == other.goID && title == other.title && hash == other.hash;
    }
}

public enum CardType
{
    Creature,
    Money,
    Development,

}
