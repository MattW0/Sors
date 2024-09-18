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

    internal async void InitFriend(Friend friend, bool isInLobby=false)
    {        
        if (!SteamClient.IsValid) return;

        try { await LoadFriendData(friend); }
        catch (Exception e) { 
            print("Failed to load friend data: "  + friend.Name);
            print(e);
            return;
        }

        _button.onClick.AddListener(() => OnSteamPlayerClicked?.Invoke(_steamId, _action));

        // Can invite friend if player is in lobby
        if (isInLobby) {
            _action = friend.IsOnline ? ButtonAction.INVITE : ButtonAction.NONE;
            _actionText.text = friend.IsOnline ? "Invite" : "";
            return;
        }

        // Otherwise, can join friends that are in a lobby
        _button.interactable = friend.IsOnline;
        Friend.FriendGameInfo? gameInfo = friend.GameInfo;
        if (gameInfo.HasValue && gameInfo.Value.Lobby.HasValue){
            _action = ButtonAction.JOIN;
            _actionText.text = "Join";
            _status.text = "In Lobby";
        }
    }

    internal async void InitLobbyMember(Friend friend, bool isHost)
    {
        print("Init lobby member");
        await LoadFriendData(friend);

        if (isHost || friend.IsMe) {
            _button.interactable = false;
            _status.text = "Host";
            _actionText.text = "";
            _action = ButtonAction.NONE;
        } else {
            _button.interactable = true;
            _status.text = "Client";
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
    }
}

public enum ButtonAction
{
    NONE,
    INVITE,
    JOIN,
    KICK
}