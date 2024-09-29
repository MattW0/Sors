using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SteamFriendsManager : MonoBehaviour
{
    [SerializeField] private SteamPlayers _steamFriends;
    [SerializeField] private TMP_Text _playerName;
    public static event Action<bool> OnFriendsInviteButtonToggle;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        SteamFriends.OnGameLobbyJoinRequested += LobbyJoinRequestSent;
    }

    public async void StartFriendScanning(int intervalMiliseconds)
    {
        _playerName.text = SteamClient.Name;

        _cts = new();
        try {
            while (true) await ScanFriends(intervalMiliseconds, _cts.Token);
        } catch (OperationCanceledException) {
            print("Friends scan cancelled");
        }
    }

    public void StopFriendScanning() => _cts.Cancel();

    private async UniTask ScanFriends(int intervalMiliseconds, CancellationToken token)
    {
        print("Scanning friends");
        token.ThrowIfCancellationRequested();
        _steamFriends.InitFriends();

        await UniTask.Delay(intervalMiliseconds, cancellationToken: token);
    }

    private void LobbyJoinRequestSent(Lobby lobby, SteamId id)
    {
        print("Lobby join request sent");
        SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        SteamFriends.OnGameLobbyJoinRequested -= LobbyJoinRequestSent;
    }
}