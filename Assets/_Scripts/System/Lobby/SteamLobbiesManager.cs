using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityUtils;
using System.Linq;
using System;
using Michsky.UI.Shift;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;


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
    [SerializeField] private ModalWindow _joinDialogue;

    private SteamFriendsManager _friendsManager;
    private Lobby _activeLobby;
    private Lobby _invitedLobby;
    private CancellationTokenSource _cts;
    private const int SCAN_INTERVAL = 3000;

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

        _friendsManager.StartFriendScanning(SCAN_INTERVAL);
        StartScanningLobbies();
    }

    private async void StartScanningLobbies()
    {
        _cts = new CancellationTokenSource();
        try {
            while (true) {
                GetLobbies(_cts.Token).Forget();
                await UniTask.Delay(SCAN_INTERVAL, cancellationToken: _cts.Token);
            }
        } catch (OperationCanceledException) {
            print("Lobbies scan cancelled");
        }
    }

    private async UniTaskVoid GetLobbies(CancellationToken token)
    {
        print ("Scanning for lobbies");
        token.ThrowIfCancellationRequested();

        var lobbies = await SteamMatchmaking.LobbyList
                                .WithMaxResults(20)
                                .FilterDistanceFar()
                                .WithSlotsAvailable(1)
                                .RequestAsync().AsUniTask();
        InstantiateLobbyItems(lobbies, token);
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
        lobby.SetData("game", "Sors");
        lobby.SetData("name", SteamClient.Name + "'s Lobby");
        lobby.SetData("owner", SteamClient.Name);

        // lobby.SetPrivate();
    }

    public void JoinLobby(SteamId lobbyId) => SteamMatchmaking.JoinLobbyAsync(lobbyId);
    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        print("Lobby joined - " + lobby.Id);
        _panelManager.OpenPanel("Lobby");
        _cts.Cancel();

        _activeLobby = lobby;
        _sorsLobby.SetLobby(lobby);
    }
    public void InvitePlayer(SteamId id) => _activeLobby.InviteFriend(id);
    public void KickPlayer(SteamId id) 
    {
        // TODO: https://wiki.facepunch.com/steamworks/SteamMatchmaking.OnLobbyMemberKicked
        print("TODO: Trying to kick player " + id.ToString()); 
    }
    public void JoinLobbyViaInvite() => JoinLobby(_invitedLobby.Id);
    private void OnInviteRecievedCallback(Friend friend, Lobby lobby)
    {
        print("Lobby invite received - " + lobby.Id + " from " + friend.Name);
        _invitedLobby = lobby;

        _joinDialogue.SetMessage("You have been invited to " + lobby.GetData("name") + " by " + friend.Name);
        _joinDialogue.ModalWindowIn();
    }

    internal void LeaveLobby()
    {
        _activeLobby.Leave();
        StartScanningLobbies();

        _panelManager.OpenPanel("Home");
    }

    internal void StartGame()
    {
        // Get player names from lobby members using LINQ
        var playerNames = _activeLobby.Members.Select(m => m.Name).ToList();
        print("Starting game with players: " + string.Join(", ", playerNames));

        _friendsManager.StopFriendScanning();
        _networkManager.StartMirror(_activeLobby.Owner.Id, SteamClient.Name);
    }

    private void InstantiateLobbyItems(Lobby[] lobbies, CancellationToken token)
    {
        _lobbiesParent.DestroyChildren();

        var header = Instantiate(_headerPrefab, _lobbiesParent);
        header.GetComponent<Header>().Init("Public Lobbies");

        foreach (var lobby in lobbies)
        {
            if (lobby.GetData("game") != "Sors") continue;

            var lobbyObject = Instantiate(_lobbyPrefab, _lobbiesParent);
            lobbyObject.GetComponent<SteamLobbyItem>().SetLobby(lobby, this);
        }
    }

    // Steam does not recognize exiting play mode : https://wiki.facepunch.com/steamworks/Installing_For_Unity#shuttingdown
    // Works fine in build mode

    // private void OnDisable()
    // {
    //     if(UnityUtils.SystemUtils.ApplicationIsAboutToExitPlayMode())
    //     {
    //         print("Shutting down Steam client");
    //         SteamClient.Shutdown();
    //     }
    // }

    private void OnDestroy()
    {
        if(_cts != null) _cts.Cancel();
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite -= OnInviteRecievedCallback;
    }

    private void OnApplicationQuit()
    {
        print("Shutting down Steam client");
        SteamClient.Shutdown();
    }
}
