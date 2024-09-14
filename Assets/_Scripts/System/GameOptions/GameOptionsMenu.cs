using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOptionsMenu : MonoBehaviour
{
    private string[] _networkAddresses = new string[2] {"localhost", "192.168.1.170"};
    public static GameOptions gameOptions = new(2, false, false, false, "", true, 4, "localhost");
    public static event Action<string> OnUpdateNetworkAddress;
    internal void SetOption(GameOption option, bool value)
    {
        if(option == GameOption.SinglePlayer) gameOptions.SinglePlayer = value;
        else if(option == GameOption.FullHand) gameOptions.FullHand = value;
        else if(option == GameOption.SaveStates) gameOptions.SaveStates = value;
        else if(option == GameOption.SkipCardSpawnAnimations) gameOptions.SkipCardSpawnAnimations = value;
    }

    internal void SetOption(GameOption option, int value)
    {
        if(option == GameOption.NumberPhases) gameOptions.NumberPhases = value;
    }

    internal void SetOption(GameOption option, string value)
    {
        if(option == GameOption.NetworkAddress) SetNetworkAddress(value);
        else if(option == GameOption.StateFile) gameOptions.StateFile = value;
    }

    private void SetNetworkAddress(string networkAddress)
    {
        var address = networkAddress.Trim();
        gameOptions.NetworkAddress = address;

        // Must be set before a player starts hosting -> from menu update NetworkManager
        OnUpdateNetworkAddress?.Invoke(address);
    }
}
