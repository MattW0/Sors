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
    public Player[] players;
    public string fileName;

    public GameState(int playerCount) 
    {
        players = new Player[playerCount];
        // Create random file name ending in .json
        fileName = Guid.NewGuid().ToString() + "_0.json";
    }

    public GameState(int playerCount, string fileName)
    {
        players = new Player[playerCount];
        this.fileName = fileName;
    }

    public void SaveState(int turnNumber)
    {
        // Replace _X with _turnNumber, where X is the last turn number
        fileName = Regex.Replace(fileName, @"_\d+", "_" + turnNumber.ToString());
        
        // convert GameState with all player data to json format, creating a new file in the resources folder
        var json = JsonUtility.ToJson(this);
        File.WriteAllText(Application.dataPath + "/Resources/GameStates/" + fileName, json);
    }
}

[System.Serializable]
public class Player
{
    public string playerName;
    public bool isHost;
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

    public void SavePlayerCardCollections(PlayerManager player)
    {
        cards.CardInfosToScriptablePathStrings(CardLocation.Deck, player.deck);
        cards.CardInfosToScriptablePathStrings(CardLocation.Hand, player.hand);
        cards.CardInfosToScriptablePathStrings(CardLocation.Discard, player.discard);
    }
}

[System.Serializable]
public class Entities : Dictionary<string, List<string>>
{
    public List<string> creatures;
    public List<string> technologies;

    public Entities()
    {
        creatures = new List<string>();
        technologies = new List<string>();
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

    public void CardInfosToScriptablePathStrings(CardLocation location, List<CardInfo> cards){
        
        List<string> scriptableCardPaths = new List<string>();
        foreach(var card in cards){
            string path = "Cards/";

            if (card.isStartCard) {
                path += "_StartCards/";
            } else {
                if(card.type == CardType.Creature) path += "CreatureCards/";
                else if(card.type == CardType.Technology) path += "TechnologyCards/";
                else if(card.type == CardType.Money) path += "MoneyCards/" + card.hash + "_";
            }

            path += card.title;
            scriptableCardPaths.Add(path);
        }
        
        switch(location){
            case CardLocation.Hand:
                handCards = scriptableCardPaths;
                break;
            case CardLocation.Deck:
                deckCards = scriptableCardPaths;
                break;
            case CardLocation.Discard:
                discardCards = scriptableCardPaths;
                break;
            default:
                break;
        }
    }
}


}