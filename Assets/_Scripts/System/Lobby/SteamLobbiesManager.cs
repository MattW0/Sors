using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityUtils;
using System.Linq;
using System;

public class SteamLobbiesManager : MonoBehaviour
{
    private SteamFriendsManager _friendsManager;
    [SerializeField] private SteamLobby _lobbyController;
    [SerializeField] private SorsSteamNetworkManager _networkManager;
    [SerializeField] private GameObject _joinDialogue;
    [SerializeField] private GameObject _lobbyPrefab;
    [SerializeField] private Transform _lobbiesParent;
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
        _lobbiesParent.DestroyChildren();

        var lobbyList = await SteamMatchmaking.LobbyList
            .WithMaxResults(10)
            .FilterDistanceClose()
            .WithSlotsAvailable(1)
            .RequestAsync();
        
        foreach (var lobby in lobbyList)
        {
            var lobbyObject = Instantiate(_lobbyPrefab, _lobbiesParent);
            lobbyObject.GetComponent<SteamLobbyItem>().SetLobby(lobby, this);
        }
    }

    public void CreateLobby() => SteamMatchmaking.CreateLobbyAsync(_networkManager.maxConnections);
    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        if (result != Result.OK) {
            print("Failed to create lobby: " + result);
            return;
        }

        lobby.SetData("name", "Lobby " + lobby.Id);
        lobby.SetData("owner", SteamClient.Name);
        print("Lobby created - " + lobby.GetData("name"));

        lobby.Owner = new Friend(SteamClient.SteamId);
    }

    public void JoinLobby(SteamId lobbyId) => SteamMatchmaking.JoinLobbyAsync(lobbyId);
    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        print("Lobby joined - " + lobby.Id);
        _activeLobby = lobby;

        _lobbyController.SetLobby(lobby);
        _friendsManager.ToggleFriendInviteButton(true);
    }
    public void InviteFriend(SteamId id) => _activeLobby.InviteFriend(id);

    public void JoinLobbyViaInvite() => JoinLobby(_invitedLobby.Id);
    private void OnInviteRecievedCallback(Friend friend, Lobby lobby)
    {
        print("Lobby invite received - " + lobby.Id + " from " + friend.Name);
        _joinDialogue.SetActive(true);
        _invitedLobby = lobby;

        _joinDialogue.GetComponent<ConfirmDialogue>().SetMessage("You have been invited to " + lobby.GetData("name") + " by " + friend.Name);
    }

    internal void LeaveLobby()
    {
        _activeLobby.Leave();
        _lobbyController.LeaveLobby();
        _friendsManager.ToggleFriendInviteButton(false);

        LoadLobbies();
    }

    private void OnDestroy()
    {
        ConfirmDialogue.OnAccept -= JoinLobbyViaInvite;
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite -= OnInviteRecievedCallback;
    }

    internal void StartGame()
    {
        // Get player names from lobby members using LINQ
        var playerNames = _activeLobby.Members.Select(m => m.Name).ToList();
        print("Starting game with players: " + string.Join(", ", playerNames));

        _networkManager.StartMirror(_activeLobby.Owner.Id, SteamClient.Name);
    }
}
