using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
using Steamworks;

public class SteamPlayers : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _headerPrefab;
    [SerializeField] private RectTransform _parent;
    [SerializeField] private bool _lobbyFriends;
    void OnEnable()
    {
        if (!SteamClient.IsValid) return;
        
        if(_lobbyFriends) InitLobbyFriends();
        else InitFriends();
    }

    public void InitFriends()
    {
        _parent.DestroyChildren();
        
        var onlinePlayers = new List<GameObject>();
        var offlinePlayers = new List<GameObject>();
        foreach (var friend in SteamFriends.GetFriends())
        {
            var steamPlayer = Instantiate(_playerPrefab);
            steamPlayer.GetComponent<SteamPlayerItem>().InitFriend(friend);

            if (friend.IsOnline) onlinePlayers.Add(steamPlayer);
            else offlinePlayers.Add(steamPlayer);
        }

        AddPlayers(onlinePlayers, offlinePlayers);
    }

    internal void AddPlayers(List<GameObject> online, List<GameObject> offline)
    {
        var header = Instantiate(_headerPrefab, _parent);
        header.GetComponent<Header>().Init("Online Friends");
        foreach (var obj in online) obj.transform.SetParent(_parent, false);

        header = Instantiate(_headerPrefab, _parent);
        header.GetComponent<Header>().Init("Offline Friends");
        foreach (var obj in offline) obj.transform.SetParent(_parent, false);
    }

    public void InitLobbyFriends()
    {
        _parent.DestroyChildren();

        var header = Instantiate(_headerPrefab, _parent);
        header.GetComponent<Header>().Init("Online Friends");

        foreach (var friend in SteamFriends.GetFriends())
        {
            if (! friend.IsOnline) continue;

            var steamPlayer = Instantiate(_playerPrefab, _parent);
            steamPlayer.GetComponent<SteamPlayerItem>().InitFriend(friend, true);
        }
    }
}
