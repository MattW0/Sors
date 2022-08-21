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

    public CardInfo(ScriptableCard card, string _goID = null)
    {
        this.hash = card.hash;
        this.title = card.title;
        this.isCreature = card.isCreature;
        this.cost = card.cost;
        this.attack = card.attack;
        this.health = card.health;

        if (_goID != null) this.goID = _goID;
        else this.goID = null;
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
