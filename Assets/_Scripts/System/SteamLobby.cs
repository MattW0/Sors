using UnityEngine;
using Mirror;
using Steamworks;
using System;

[RequireComponent(typeof(SorsNetworkManager))]
public class SteamLobby : MonoBehaviour
{
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    public ulong CurrentLobbyID;
    public string HostAddressKey = "HostAddress";
    private SorsNetworkManager _networkManager;
    [SerializeField] private GameObject _hostButton;

    void Start()
    {
        if(!SteamManager.Initialized)
        {
            Debug.LogError("Steamworks not initialized");
            return;
        }

        _networkManager = GetComponent<SorsNetworkManager>();
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Failed to create lobby");
            return;
        }

        CurrentLobbyID = callback.m_ulSteamIDLobby;
        print("Lobby created: " + CurrentLobbyID);

        _networkManager.StartHost();
        _hostButton.SetActive(false);
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("Request to join lobby: " + callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if(NetworkServer.active) return;

        _networkManager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        _networkManager.StartClient();
    }
}
