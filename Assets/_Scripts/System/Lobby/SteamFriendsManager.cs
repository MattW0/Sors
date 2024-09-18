using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using System.Collections.Generic;
using UnityUtils;
using TMPro;

public class SteamFriendsManager : MonoBehaviour
{
    [SerializeField] private SteamPlayers _steamFriends;
    [SerializeField] private TMP_Text _playerName;
    public static event Action<bool> OnFriendsInviteButtonToggle;

    private void Start()
    {
        if (!SteamClient.IsValid) return;

        SteamFriends.OnGameLobbyJoinRequested += LobbyJoinRequestSent;
        
        _playerName.text = SteamClient.Name;
        _steamFriends.InitFriends();
    }

    private void LobbyJoinRequestSent(Lobby lobby, SteamId id)
    {
        print("Lobby join request sent");
        // SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }
}