using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameState
{
public class GameState
{
    public Player[] players;
}

[System.Serializable]
public class Player
{
    public string playerName;
    public bool isHost;
    public Entities[] entities;
    public Cards cards;
}

[System.Serializable]
public class Entities
{
    public List<string> creatures;
    public List<string> technologies;
}

[System.Serializable]
public class Cards : Dictionary<string, List<string>>
{
    public List<string> handCards;
    public List<string> deckCards;
    public List<string> discardCards;
}

}