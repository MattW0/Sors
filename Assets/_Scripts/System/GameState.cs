using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameState
{
public class GameState
{
    public PlayerState[] playerStates;
}

public class PlayerState
{
    public Entities[] entities;
}

public class Entities
{
    public Dictionary<string, List<string>> creatures;
    public Dictionary<string, List<string>> technologies;
}

}