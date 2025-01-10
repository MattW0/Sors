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
    public int winScore;

    // Initial Deck
    public int initialDeckSize;
    public int initialEntities;

    // Default turn recources
    public int cardDraw;
    public int buys;
    public int plays;
    public int prevails;

    // Phase Boni
    public int phaseDiscard;
    public int extraDraw;
    public int extraBuys;
    public int extraPlays;
    public int extraPrevails;
    public int extraCash;
    public int marketPriceReduction;

    public GameOptions(int numPhases = 2,
                       bool singlePlayer = false, 
                       bool fullHand = false, 
                       bool skipSpawnimations = false, 
                       string stateFile = "", 
                       bool saveStates = true, 
                       int initialHandSize = 4, 
                       string address = "localhost")
    {
        // Player stats
        startHealth = 10;
        winScore = 10;

        // Initial Deck
        initialDeckSize = 10;
        initialEntities = 4;

        // Default turn recources
        cardDraw = 2;
        buys = 1;
        plays = 1;
        prevails = 1;
        phaseDiscard = 1;

        // Phase Boni
        extraDraw = 1;
        extraBuys = 1;
        extraPlays = 1;
        extraPrevails = 2;
        extraCash = 1;
        marketPriceReduction = 1;

        // Configurable options
        NumberPhases = numPhases;
        SinglePlayer = singlePlayer;
        SaveStates = saveStates;
        SkipCardSpawnAnimations = skipSpawnimations;

        NetworkAddress = address;
        StateFile = stateFile;

        FullHand = fullHand;
        if (fullHand) InitialHandSize = initialDeckSize;
        else InitialHandSize = initialHandSize;
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
    SaveStates
}