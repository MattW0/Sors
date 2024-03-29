using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CardInfo
{
    public string goID;
    public string title;
    public string hash;
    // public Sprite image;

    [Header("Money properties")]
    public bool isCreature;
    public int moneyValue;

    [Header("Creature properties")]
    public int cost;
    public int attack;
    public int health;
    public int points;
    public List<Keywords> keyword_abilities;

    public CardInfo(ScriptableCard card, string gameObjectID = null)
    {
        hash = card.hash;
        title = card.title;
        cost = card.cost;
        // image = card.image;

        if (gameObjectID != null) goID = gameObjectID;
        else goID = null;

        isCreature = card.isCreature;
        moneyValue = card.moneyValue;

        // Should split money and creature cards into separate scriptable objects
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

        this.isCreature = false;
        this.moneyValue = 0;

        this.keyword_abilities = null;
    }
}
