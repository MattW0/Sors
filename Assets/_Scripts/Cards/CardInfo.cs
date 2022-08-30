using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CardInfo
{
    public string hash;
    public string title;
    public bool isCreature;
    public int cost;
    public int attack;
    public int health;
    public string goID;

    public CardInfo(ScriptableCard card, string gameObjectID = null)
    {
        hash = card.hash;
        title = card.title;
        isCreature = card.isCreature;
        cost = card.cost;
        attack = card.attack;
        health = card.health;

        if (gameObjectID != null) goID = gameObjectID;
        else goID = null;
    }

    public void Destroy(){
        this.hash = null;
        this.title = null;
        this.isCreature = false;
        this.cost = 0;
        this.attack = 0;
        this.health = 0;
        this.goID = null;
    }
}
