using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityUtils;
using System.Linq;
using System;
using Michsky.UI.Shift;

public class SteamLobbiesManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SorsSteamNetworkManager _networkManager;
    [SerializeField] private MainPanelManager _panelManager;
    [SerializeField] private LobbySors _sorsLobby;

    [Header("UI")]
    [SerializeField] private GameObject _headerPrefab;
    [SerializeField] private GameObject _lobbyPrefab;
    [SerializeField] private Transform _lobbiesParent;
    [SerializeField] private GameObject _joinDialogue;

    private SteamFriendsManager _friendsManager;
    private Lobby _activeLobby;
    private Lobby _invitedLobby;

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        _friendsManager = GetComponent<SteamFriendsManager>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite += OnInviteRecievedCallback;
    }

    private void Start()
    {
        if (!SteamClient.IsValid)
        {
            print("Steam client not valid");
            return;
        }
        if (!SteamClient.IsLoggedOn)
        {
            print("Steam client not logged on");
            return;
        }

        if (_networkManager == null)
        {
            print("Network manager does not exist on the same game object");
            return;
        }

        LoadLobbies();
    }

    private async void LoadLobbies()
    {
        try
        {
            var lobbies = await SteamMatchmaking.LobbyList
                                .WithMaxResults(10)
                                .FilterDistanceClose()
                                .WithSlotsAvailable(1)
                                .RequestAsync();
            InstantiateLobbyItems(lobbies);
        }
        catch (Exception e) {
            print("Failed to load lobby data");
            print(e);
        }
    }

    public void CreateLobby() => SteamMatchmaking.CreateLobbyAsync(_networkManager.maxConnections);
    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        print("Lobby created - " + lobby.Id);
        if (result != Result.OK) {
            print("Failed to create lobby: " + result);
            return;
        }

        lobby.Owner = new Friend(SteamClient.SteamId);
        lobby.SetData("name", SteamClient.Name + "'s Lobby");
        lobby.SetData("owner", SteamClient.Name);

        // lobby.SetPrivate();
    }

    public void JoinLobby(SteamId lobbyId) => SteamMatchmaking.JoinLobbyAsync(lobbyId);
    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        print("Lobby joined - " + lobby.Id);
        _panelManager.OpenPanel("Lobby");

        _activeLobby = lobby;
        _sorsLobby.SetLobby(lobby);
    }
    public void InvitePlayer(SteamId id) => _activeLobby.InviteFriend(id);
    public void KickPlayer(SteamId id) {}//TODO: _activeLobby.Kick(id);
    public void JoinLobbyViaInvite() => JoinLobby(_invitedLobby.Id);
    private void OnInviteRecievedCallback(Friend friend, Lobby lobby)
    {
        print("Lobby invite received - " + lobby.Id + " from " + friend.Name);
        _joinDialogue.SetActive(true);
        _invitedLobby = lobby;

        _joinDialogue.GetComponent<ModalWindow>().SetMessage("You have been invited to " + lobby.GetData("name") + " by " + friend.Name);
    }

    internal void LeaveLobby()
    {
        _activeLobby.Leave();
        LoadLobbies();

        _panelManager.OpenPanel("Home");
    }

    internal void StartGame()
    {
        // Get player names from lobby members using LINQ
        var playerNames = _activeLobby.Members.Select(m => m.Name).ToList();
        print("Starting game with players: " + string.Join(", ", playerNames));

        _networkManager.StartMirror(_activeLobby.Owner.Id, SteamClient.Name);
    }

    private void InstantiateLobbyItems(Lobby[] lobbies)
    {
        _lobbiesParent.DestroyChildren();

        var header = Instantiate(_headerPrefab, _lobbiesParent);
        header.GetComponent<Header>().Init("Public Lobbies");

        foreach (var lobby in lobbies)
        {
            var lobbyObject = Instantiate(_lobbyPrefab, _lobbiesParent);
            lobbyObject.GetComponent<SteamLobbyItem>().SetLobby(lobby, this);
        }
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite -= OnInviteRecievedCallback;
    }
}
