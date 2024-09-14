using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityUtils;

public class SteamFriendsManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private RectTransform _friendsParent;
    [SerializeField] private RectTransform _friendsOfflineParent;
    public static event Action<bool> OnFriendsInviteButtonToggle;

    private void Start()
    {
        if (!SteamClient.IsValid) return;

        SteamFriends.OnGameLobbyJoinRequested += LobbyJoinRequestSent;
        
        InitFriends();
    }

    private void LobbyJoinRequestSent(Lobby lobby, SteamId id)
    {
        print("Lobby join request sent");
        // SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    public void InitFriends()
    {
        _friendsParent.DestroyChildren();
        
        foreach (var friend in SteamFriends.GetFriends())
        {
            SteamPlayerItem steamPlayer;
            if (friend.IsOnline) steamPlayer = Instantiate(_playerPrefab, _friendsParent).GetComponent<SteamPlayerItem>();
            else steamPlayer = Instantiate(_playerPrefab, _friendsOfflineParent).GetComponent<SteamPlayerItem>();

            steamPlayer.InitFriend(friend);
        }
    }

    // How to handle event subscription when updating (deleting) SteamPlayerItems ?
    internal void ToggleFriendInviteButton(bool v) => OnFriendsInviteButtonToggle?.Invoke(v);
}