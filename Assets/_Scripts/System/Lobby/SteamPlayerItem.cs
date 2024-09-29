using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Threading.Tasks;
using System;

public class SteamPlayerItem : MonoBehaviour
{
    private SteamId _steamId;
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private TMP_Text _status;
    [SerializeField] private TMP_Text _actionText;
    [SerializeField] private RawImage _playerAvatar;
    [SerializeField] private Button _button;
    private ButtonAction _action;
    public static event Action<SteamId, ButtonAction> OnSteamPlayerClicked;

    internal async void InitFriend(Friend friend, bool selfIsInLobby=false)
    {        
        if (!SteamClient.IsValid) return;

        try { await LoadFriendData(friend); }
        catch (Exception e) { 
            print("Failed to load friend data: "  + friend.Name);
            print(e);
            return;
        }

        _button.onClick.AddListener(() => OnSteamPlayerClicked?.Invoke(_steamId, _action));

        // Can invite friend if local player (self) is in lobby. Otherwise, can join friends that are in a lobby
        if (selfIsInLobby) InitLobbyFriend(friend.IsOnline);
        else _button.interactable = friend.IsOnline;
        
        // TODO: Is it possible to join a friend that is hosting a public Sors lobby?
        // InitDefaultFriend(friend); Doesn't work
        // friend.IsPlayingThisGame could be interesting (but needs proper SteamID for Sors)
    }

    private void InitLobbyFriend(bool friendIsOnline)
    {
        _action = friendIsOnline ? ButtonAction.INVITE : ButtonAction.NONE;
        _actionText.text = friendIsOnline ? "Invite" : "";
    }

    private void InitDefaultFriend(Friend friend)
    {
        
        Friend.FriendGameInfo? gameInfo = friend.GameInfo;
        if (gameInfo.HasValue && gameInfo.Value.Lobby.HasValue){
            print("Friend game info: " + gameInfo.Value.Lobby);
            _action = ButtonAction.JOIN;
            _actionText.text = "Join";
            _status.text = "In Lobby";
        }
    }

    internal async void InitLobbyMember(Friend friend, bool isHost)
    {
        await LoadFriendData(friend);

        _button.interactable = false;
        _status.text = isHost ? "Host" : "Client";

        if (friend.IsMe) _actionText.text = "";
        else if (! isHost) {
            _button.interactable = true;
            _actionText.text = "Kick";
            _action = ButtonAction.KICK;
        }
    }

    private async Task LoadFriendData(Friend friend)
    {
        _steamId = friend.Id;
        _playerName.text = friend.Name;
        _status.text = friend.IsOnline ? "Online" : "Offline";

        _playerAvatar.texture = await SteamDataWrapper.GetTextureFromSteamIdAsync(_steamId);
    }

    internal void Reset()
    {
        _playerName.text = "";
        _status.text = "";
        _actionText.text = "";
        _action = ButtonAction.NONE;
        _playerAvatar.texture = null;
    }
}

public enum ButtonAction
{
    NONE,
    INVITE,
    JOIN,
    KICK
}