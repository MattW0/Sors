using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameOptions
{
    public string NetworkAddress { get; set; }
    public int NumberPhases { get; set; }
    public bool SinglePlayer { get; set; }
    public bool FullHand { get; set; }
    public bool SkipCardSpawnAnimations { get; set; }
    public string StateFile { get; set; }
    public bool SaveStates { get; set; }
    public int InitialHandSize { get; set; }

    // Player stats
    public int startHealth;
    public int startScore;

    // Initial Deck
    public int initialDeckSize;
    public int initialEntities;

    // Default turn recources
    public int turnCardDraw;
    public int turnCash;
    public int turnBuys;
    public int turnPlays;
    public int turnPrevails;

    // Phase Boni
    public int phaseDiscard;
    public int extraDraw;
    public int extraCash;
    public int extraBuys;
    public int extraPlays;
    public int extraPrevails;

    public GameOptions(string address, int numPhases, bool singlePlayer, bool fullHand, bool skipSpawnimations, string stateFile, bool saveStates, int initialHandSize)
    {
        NetworkAddress = address;
        NumberPhases = numPhases;
        SinglePlayer = singlePlayer;
        FullHand = fullHand;
        SkipCardSpawnAnimations = skipSpawnimations;
        StateFile = stateFile;
        InitialHandSize = initialHandSize;
        SaveStates = saveStates;

        // Player stats
        startHealth = 10;
        startScore = 0;

        // Initial Deck
        initialDeckSize = 10;
        initialEntities = 4;

        // Default turn recources
        turnCardDraw = 2;
        turnCash = 0;
        turnBuys = 0;
        turnPlays = 0;
        turnPrevails = 0;
        phaseDiscard = 1;

        // Phase Boni
        extraDraw = 2;
        extraBuys = 1;
        extraPlays = 1;
        extraCash = 3;
        extraPrevails = 2;
    }

    public override string ToString()
    {
        var phases = $"Number phases: {NumberPhases}\n";
        var players = $"Single player: {SinglePlayer}\n";
        var fullHand = $"Full hand: {FullHand}\n";
        var spawnimations = $"Skip spawn animations: {SkipCardSpawnAnimations}\n";
        var address = $"Host address: {NetworkAddress}\n";
        var initialHandSize = $"Initial hand size: {InitialHandSize}\n";
        var state = $"Saving states: {SaveStates}\n";
        if (StateFile != "") state += $"Loading from state: {StateFile}\n";

        return address + phases + players + fullHand + initialHandSize + spawnimations + state;
    } 
}

public enum GameOption : byte
{
    NumberPhases,
    NetworkAddress,
    StateFile,
    SinglePlayer,
    FullHand,
    SkipCardSpawnAnimations,
}