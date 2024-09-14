using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityUtils;

public class LobbyMembers : MonoBehaviour
{
    [SerializeField] private RectTransform _membersParent;
    [SerializeField] private GameObject _playerPrefab;

    public void InitLobby(Lobby lobby)
    {
        _membersParent.DestroyChildren();
        foreach(var member in lobby.Members) AddMember(member);
    }

    public void AddMember(Friend friend)
    {
        var steamPlayer = Instantiate(_playerPrefab, _membersParent).GetComponent<SteamPlayerItem>();
        steamPlayer.InitLobbyMember(friend);
    }

    internal void ClearMembers() => _membersParent.DestroyChildren();
}
