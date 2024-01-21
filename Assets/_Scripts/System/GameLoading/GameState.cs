using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SorsGameState
{
public class GameState
{
    public int turn;
    public string fileName;
    public Market market;
    public Player[] players;

    public GameState(int playerCount) 
    {
        market = new Market();
        players = new Player[playerCount];
        // Create fileName from current time
        fileName = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + "_0.json";
    }

    public GameState(int playerCount, string fileName)
    {
        market = new Market();
        players = new Player[playerCount];
        this.fileName = fileName;
    }

    public void SaveState(int turnNumber)
    {
        turn = turnNumber;
        // Replace _X with _turnNumber, where X is the last turn number
        fileName = Regex.Replace(fileName, @"_\d+", "_" + turnNumber.ToString());
        
        // convert GameState with all player data to json format, creating a new file in the resources folder
        var json = JsonUtility.ToJson(this);
        File.WriteAllText(Application.dataPath + "/Resources/GameStates/Auto/" + fileName, json);
    }

    public static List<string> CardInfosToScriptablePathStrings(List<CardInfo> cards)
    {    
        List<string> scriptableCardPaths = new List<string>();
        foreach(var card in cards) scriptableCardPaths.Add(GetScriptableObjectPath(card));
        
        return scriptableCardPaths;
    }

    public static string GetScriptableObjectPath(CardInfo card)
    {
        string path = "Cards/";

        if (card.isStartCard) {
            path += "_StartCards/";
        } else {
            if(card.type == CardType.Creature) path += "CreatureCards/";
            else if(card.type == CardType.Technology) path += "TechnologyCards/";
            else if(card.type == CardType.Money) path += "MoneyCards/";
        }

        path += card.resourceName;
        return path;
    }
}

[System.Serializable]
public class Market
{
    public List<string> money;
    public List<string> technologies;
    public List<string> creatures;

    public Market() 
    {
        money = new List<string>();
        technologies = new List<string>();
        creatures = new List<string>();
    }

    public Market(List<string> money, List<string> technologies, List<string> creatures)
    {
        this.money = money;
        this.technologies = technologies;
        this.creatures = creatures;
    }

    public void SaveMarketState(List<CardInfo>[] market)
    {
        money = GameState.CardInfosToScriptablePathStrings(market[0]);
        technologies = GameState.CardInfosToScriptablePathStrings(market[1]);
        creatures = GameState.CardInfosToScriptablePathStrings(market[2]);
    }
}

[System.Serializable]
public class Player
{
    public string playerName;
    public bool isHost;
    public int health;
    public Entities entities;
    public Cards cards;

    public Player() {}

    public Player(string playerName, bool isHost)
    {
        this.playerName = playerName;
        this.isHost = isHost;
        entities = new Entities();
        cards = new Cards();
    }

    public void SavePlayerState(PlayerManager player)
    {
        health = player.Health;
        cards.deckCards = GameState.CardInfosToScriptablePathStrings(player.deck);
        cards.handCards = GameState.CardInfosToScriptablePathStrings(player.hand);
        cards.discardCards = GameState.CardInfosToScriptablePathStrings(player.discard);
    }
}

[System.Serializable]
public class Entities : Dictionary<string, List<Entity>>
{
    public List<Entity> creatures;
    public List<Entity> technologies;

    public Entities()
    {
        creatures = new List<Entity>();
        technologies = new List<Entity>();
    }
}

[System.Serializable]
public class Entity
{
    public string scriptableCard;
    public int health;
    public int attack;

    // For technologies
    public Entity(CardInfo card, int h)
    {
        scriptableCard = GameState.GetScriptableObjectPath(card);
        health = h;
    }

    // For creatures
    public Entity(CardInfo card, int h, int a)
    {
        scriptableCard = GameState.GetScriptableObjectPath(card);
        health = h;
        attack = a;
    }
}

[System.Serializable]
public class Cards : Dictionary<string, List<string>>
{
    public List<string> handCards;
    public List<string> deckCards;
    public List<string> discardCards;

    public Cards()
    {
        handCards = new List<string>();
        deckCards = new List<string>();
        discardCards = new List<string>();
    }
}


}