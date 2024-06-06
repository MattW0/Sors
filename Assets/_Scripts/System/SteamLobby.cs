using UnityEngine;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    public ulong CurrentLobbyID;
    public string HostAddressKey = "HostAddress";
    private SteamNetworkManager _networkManager;
    // [SerializeField] private GameObject _hostButton;
    // [SerializeField] private TMP_Text _lobbyNameText;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    void Start()
    {
        if(!SteamManager.Initialized)
        {
            Debug.LogError("Steamworks not initialized");
            return;
        }

        _networkManager = GetComponent<SteamNetworkManager>();
        // _hostButton.GetComponent<Button>().onClick.AddListener(HostLobby);
        
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        lobbyMatchList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _networkManager.maxConnections);
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
        // _hostButton.SetActive(false);
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString());
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("Request to join lobby: " + callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // _hostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        // _lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name") + "'s Lobby";

        // Clients
        if(NetworkServer.active) return;

        _networkManager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        _networkManager.StartClient();
    }

    #region Part 3

    protected Callback<LobbyMatchList_t> lobbyMatchList;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdate;
    public List<CSteamID> lobbyIds = new List<CSteamID>();

    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        LobbiesManager.Instance.DisplayLobbies(lobbyIds, result);
    }

    private void OnGetLobbyList(LobbyMatchList_t result)
    {
        if(LobbiesManager.Instance.lobbiesInstances.Count > 0) LobbiesManager.Instance.ClearLobbyDataEntries();

        for(int i=0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIds.Add(lobbyId);
            SteamMatchmaking.RequestLobbyData(lobbyId);
        }
    }

    public void GetLobbies()
    {
        if(lobbyIds.Count > 0) lobbyIds.Clear();

        // TODO: Change filter?
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose);
        SteamMatchmaking.RequestLobbyList();
    }

    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    #endregion
}
