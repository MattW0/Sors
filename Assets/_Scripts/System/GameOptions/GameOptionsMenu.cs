using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOptionsMenu : MonoBehaviour
{
    private string[] _networkAddresses = new string[2] {"localhost", "192.168.1.170"};
    public static GameOptions gameOptions = new("localhost", 2, false, false, false, "", true, 4);
    public static event Action<string> OnUpdateNetworkAddress;
    public static void SetNumberPhases(int numberPhases) => gameOptions.NumberPhases = numberPhases + 1;
    public static void SetFullHand(bool drawAll){
        gameOptions.FullHand = drawAll;
        gameOptions.InitialHandSize = drawAll ? 10 : 4;
    }
    public static void SetSinglePlayer(bool b) => gameOptions.SinglePlayer = b;
    public static void SetSaveStates(bool b) => gameOptions.SaveStates = b;
    public static void SetSpawnimations(bool b) => gameOptions.SkipCardSpawnAnimations = b;
    public static void SetStateFile(string stateFile) => gameOptions.StateFile = stateFile;
    public static void SetNetworkAddress(string networkAddress)
    {
        var address = networkAddress.Trim();
        gameOptions.NetworkAddress = address;

        // Must be set before a player starts hosting -> from menu update NetworkManager
        OnUpdateNetworkAddress?.Invoke(address);
    }
}
