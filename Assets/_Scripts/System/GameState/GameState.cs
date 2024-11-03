using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace SorsGameState
{
public class GameState
{
    public string fileName;
    public int turn;
    public Market market;
    public Player[] players;
    private readonly string _dataDirPath = Application.persistentDataPath;
    private const string _testStatesDir = "TestStates";

    public GameState(int playerCount) 
    {
        // Create fileName from current time
        fileName = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + "_0.json";

        market = new Market();
        players = new Player[playerCount];
    }

    public GameState(int playerCount, string fileName)
    {
        this.fileName = fileName + ".json";

        market = new Market();
        players = new Player[playerCount];
    }

    public GameState LoadState()
    {
        var dirPath = _dataDirPath;
        // check if file does not start with time stamp -> load test state
        if(!Regex.IsMatch(fileName, @"^\d{4}-\d{2}-\d{2}--\d{2}-\d{2}-\d{2}"))
        {
            dirPath = Path.Combine(_dataDirPath, _testStatesDir);
        }
        var fullPath = Path.Combine(dirPath, fileName);

        try
        {
            string dataToLoad = "";
            using (FileStream stream = new(fullPath, FileMode.Open))
            {
                using StreamReader reader = new(stream);
                dataToLoad = reader.ReadToEnd();
            }

            return JsonUtility.FromJson<GameState>(dataToLoad);
        }
        catch
        {
            Debug.LogError($"Could not find or read file: {fullPath}");
            return null;
        }
    }

    public void SaveState(int turnNumber)
    {
        turn = turnNumber;
        // Replace _X with _turnNumber, where X is the last turn number
        fileName = Regex.Replace(fileName, @"_\d+", "_" + turnNumber.ToString());
        var fullPath = Path.Combine(_dataDirPath, fileName);

        try
        {
            // Create directory if it does not exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // convert GameState with all player data to json format
            var json = JsonUtility.ToJson(this, true);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                }
            }
        }
        catch
        {
            Debug.LogError($"GameState: Could not save game state to file: {fullPath}");
        }

        // File.WriteAllText(Application.dataPath + "/Resources/GameStates/Auto/" + fileName, json);
    }

    public static List<string> CardInfosToScriptablePaths(List<CardInfo> cards)
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
        money = GameState.CardInfosToScriptablePaths(market[0]);
        technologies = GameState.CardInfosToScriptablePaths(market[1]);
        creatures = GameState.CardInfosToScriptablePaths(market[2]);
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

        // Lists should be CardInfo because we need the same function to get market tiles CardInfos
        // and they have no CardStats component.
        cards.deckCards = GameState.CardInfosToScriptablePaths(player.deck.Select(c => c.cardInfo).ToList());
        cards.handCards = GameState.CardInfosToScriptablePaths(player.hand.Select(c => c.cardInfo).ToList());
        cards.discardCards = GameState.CardInfosToScriptablePaths(player.discard.Select(c => c.cardInfo).ToList());
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