using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System;

public class PlayerObjectController : NetworkBehaviour
{

    [SyncVar] public int connectionId;
    [SyncVar] public int playerIdNumber;
    [SyncVar] public ulong playerSteamId;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;

    private SteamNetworkManager _networkManager;
    private SteamNetworkManager Manager
    {
        get
        {
            if (_networkManager != null) { return _networkManager; }
            return _networkManager = SteamNetworkManager.singleton as SteamNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdUpdatePlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalPlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    public void CmdUpdatePlayerName(string newName)
    {
        this.PlayerNameUpdate(this.playerName, newName);
    }

    private void PlayerNameUpdate(string oldValue, string newValue)
    {
        if(isServer)
        {
            this.playerName = newValue;
        }
        if(isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

}
