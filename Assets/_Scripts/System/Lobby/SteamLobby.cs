using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using System;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private SteamLobbiesManager _steamLobbiesManager;
    private LobbyMembers _lobbyMembers;

    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private TMP_Text _lobbyId;
    [SerializeField] private TMP_Text _lobbyName;
    [SerializeField] private TMP_Text _lobbyOwnerName;

    private void Awake() 
    {
        _lobbyMembers = GetComponent<LobbyMembers>();
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberLeave += OnMemeberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoinCallback;
    }

    public void SetLobby(Lobby lobby)
    {
        _lobbyScreen.SetActive(true);

        _lobbyId.text = lobby.Id.ToString();
        _lobbyName.text = lobby.GetData("name");
        _lobbyOwnerName.text = lobby.Owner.Name;

        _lobbyMembers.InitLobby(lobby);
    }

    private void OnMemberJoinCallback(Lobby lobby, Friend friend)
    {
        print($"{friend.Name} joined");
        _lobbyMembers.AddMember(friend);
    }

    private void OnMemeberLeaveCallback(Lobby lobby, Friend friend)
    {
        print($"{friend.Name} left lobby");
        _lobbyMembers.InitLobby(lobby);
    }

    public void LeaveLobby()
    {
        _lobbyScreen.SetActive(false);
    }
}
