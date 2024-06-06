using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private GameObject _playerListView;
    [SerializeField] private GameObject _playerListEntryPrefab;
    [SerializeField] private GameObject _localPlayerObject;

    public ulong CurrentLobbyID;
    public bool playerItemCreated;
    private List<PlayerDataEntry> _playerDataEntries = new List<PlayerDataEntry>();
    public PlayerObjectController playerObjectController;

    private SteamNetworkManager _networkManager;
    private SteamNetworkManager Manager
    {
        get
        {
            if (_networkManager != null) { return _networkManager; }
            return _networkManager = SteamNetworkManager.singleton as SteamNetworkManager;
        }
    }

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        _lobbyNameText.text = SteamMatchmaking.GetLobbyData((CSteamID)CurrentLobbyID, "name");
    }

    public void UpdatePlayerList()
    {
        if(!playerItemCreated) CreateHostPlayerItem();
        if(_playerDataEntries.Count < Manager.GamePlayers.Count)
        {
            CreateClientPlayerItem();
        }
        else if(_playerDataEntries.Count > Manager.GamePlayers.Count)
        {
            RemovePlayerItem();
        }
        else
        {
            UpdatePlayerItem();
        }
    }

    public void FindLocalPlayer()
    {
        playerObjectController = GameObject.Find("LocalPlayer").GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(var player in Manager.GamePlayers)
        {
            var playerDataEntry = Instantiate(_playerListEntryPrefab, _playerListView.transform) as GameObject;
            var playerDataEntryComponent = playerDataEntry.GetComponent<PlayerDataEntry>();

            playerDataEntryComponent.playerName = player.playerName;
            playerDataEntryComponent.connectionId = player.connectionId;
            playerDataEntryComponent.playerSteamId = player.playerSteamId;
            playerDataEntryComponent.SetPlayerData();

            _playerDataEntries.Add(playerDataEntryComponent);
        }

        playerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach(var player in Manager.GamePlayers)
        {
            if(_playerDataEntries.Exists(x => x.connectionId == player.connectionId)) continue;

            var playerDataEntry = Instantiate(_playerListEntryPrefab, _playerListView.transform) as GameObject;
            var playerDataEntryComponent = playerDataEntry.GetComponent<PlayerDataEntry>();

            playerDataEntryComponent.playerName = player.playerName;
            playerDataEntryComponent.connectionId = player.connectionId;
            playerDataEntryComponent.playerSteamId = player.playerSteamId;
            playerDataEntryComponent.SetPlayerData();

            _playerDataEntries.Add(playerDataEntryComponent);
        }
    }

    public void UpdatePlayerItem()
    {
        foreach(var player in Manager.GamePlayers)
        {
            foreach(var playerDataEntry in _playerDataEntries)
            {
                if(playerDataEntry.connectionId != player.connectionId) continue;
                
                playerDataEntry.playerName = player.playerName;
                playerDataEntry.SetPlayerData();
            }
        }
    }

    public void RemovePlayerItem()
    {
        List<PlayerDataEntry> playerDataEntriesToRemove = new();

        foreach(var playerDataEntry in _playerDataEntries)
        {
            if(Manager.GamePlayers.Exists(x => x.connectionId == playerDataEntry.connectionId)) continue;

            playerDataEntriesToRemove.Add(playerDataEntry);
        }

        if(playerDataEntriesToRemove.Count == 0) return;
        foreach(var playerDataEntry in playerDataEntriesToRemove)
        {
            _playerDataEntries.Remove(playerDataEntry);
            Destroy(playerDataEntry.gameObject);
        }
    }
}
