using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameOptions{

    public int NumberPlayers { get; set;}
    public int NumberPhases { get; set;}
    public bool FullHand { get; set;}
    public bool SkipCardSpawnAnimations { get; set;}
    public string NetworkAddress { get; set;}
    public string StateFile { get; set;}
    public GameOptions(int numPlayers, int numPhases, bool fullHand, bool skipSpawnimations, string address, string stateFile){
        NumberPlayers = numPlayers;
        NumberPhases = numPhases;
        FullHand = fullHand;
        SkipCardSpawnAnimations = skipSpawnimations;
        NetworkAddress = address;
        StateFile = stateFile;
    }

    public override string ToString()
    {
        var players = $"Number players: {NumberPlayers}\n";
        var phases = $"Number phases: {NumberPhases}\n";
        var fullHand = $"Full hand: {FullHand}\n";
        var spawnimations = $"Skip spawn animations: {SkipCardSpawnAnimations}\n";
        var address = $"Host address: {NetworkAddress}\n";
        var state = "";
        if (StateFile != "") state = $"Loading from state: {StateFile}\n";

        return address + state + players + phases + fullHand + spawnimations;
    } 
}

public enum GameOption : byte
{
    NumberPlayers,
    NumberPhases,
    NetworkAddress,
    StateFile
}