using UnityEngine;
using Steamworks.Data;
using Steamworks;
using TMPro;

public class LobbySors : MonoBehaviour
{
    [SerializeField] private SteamPlayerItem _host;
    [SerializeField] private SteamPlayerItem _client;
    [SerializeField] private TMP_Text _lobbyId;
    [SerializeField] private TMP_Text _lobbyName;
    [SerializeField] private TMP_Text _lobbyOwnerName;
    private Lobby _lobby;

    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberLeave += OnMemeberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoinCallback;
    }

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyId.text = lobby.Id.ToString();
        _lobbyName.text = lobby.GetData("name");
        _lobbyOwnerName.text = lobby.Owner.Name;

        InitLobby();
    }

    private void InitLobby()
    {
        _host.Reset();
        _client.Reset();

        foreach(var member in _lobby.Members) 
            if (member.Id == _lobby.Owner.Id) _host.InitLobbyMember(member, true);
            else _client.InitLobbyMember(member, false);
    }

    private void OnMemberJoinCallback(Lobby lobby, Friend friend)
    {
        if (lobby.Id != _lobby.Id) return;

        print($"{friend.Name} joined");
        InitLobby();
    }

    private void OnMemeberLeaveCallback(Lobby lobby, Friend friend)
    {
        if (lobby.Id != _lobby.Id) return;

        print($"{friend.Name} left lobby");
        InitLobby();
    }
}
