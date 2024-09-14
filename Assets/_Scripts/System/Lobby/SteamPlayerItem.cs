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
    public static event Action<SteamId> OnInvite;
    public static event Action<SteamId> OnJoin;

    private SteamId _steamId;
    public SteamId SteamId 
    { 
        get => _steamId; 
        set 
        { 
            _steamId = value; 
            _playerId.text = value.ToString(); 
        } 
    }
    public string PlayerName { get => _playerName.text; set => _playerName.text = value; }
    [SerializeField] private bool _isSelf = false;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TMP_Text _playerId;
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private RawImage _playerAvatar;
    [SerializeField] private Button _inviteButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _kickButton;

    private void Start()
    {   
        if (!SteamClient.IsValid) return;
        
        _inviteButton.onClick.AddListener(() => Invite());
        _joinButton.onClick.AddListener(() => Join());

        _inviteButton.gameObject.SetActive(false);
        
        if (! _isSelf) return;

        InitPlayer(SteamClient.SteamId, SteamClient.Name);
        _joinButton.gameObject.SetActive(false);
    }

    private async void InitPlayer(SteamId id, string name)
    {
        SteamId = id;
        PlayerName = name;

        _playerAvatar.texture = await GetTextureFromSteamIdAsync(SteamId);
    }

    internal async void InitFriend(Friend friend)
    {
        await LoadFriendData(friend);

        if(! friend.IsOnline) _backgroundImage.color = Color.black;

        Friend.FriendGameInfo? gameInfo = friend.GameInfo;
        _joinButton.interactable = (gameInfo.HasValue && gameInfo.Value.Lobby.HasValue);
        
        SteamFriendsManager.OnFriendsInviteButtonToggle += (bool b) => _inviteButton.gameObject.SetActive(b);
    }

    internal async void InitLobbyMember(Friend friend)
    {
        await LoadFriendData(friend);

        _joinButton.gameObject.SetActive(false);

        if (friend.IsMe) _backgroundImage.color = Color.green;
        else _kickButton.gameObject.SetActive(true);
    }

    private async Task LoadFriendData(Friend friend)
    {
        SteamId = friend.Id;
        PlayerName = friend.Name;

        _playerAvatar.texture = await GetTextureFromSteamIdAsync(SteamId);
    }

    public void Invite()
    {
        Debug.Log("Invited " + SteamId);
        OnInvite?.Invoke(SteamId);
    }

    public void Join()
    {
        Debug.Log("Joining " + SteamId);
        OnJoin?.Invoke(SteamId);
    }

    private static async Task<Texture2D> GetTextureFromSteamIdAsync(SteamId id)
    {
        var img = await SteamFriends.GetLargeAvatarAsync(id);
        return GetTextureFromImage(img.Value);
    }

    private static Texture2D GetTextureFromImage(Steamworks.Data.Image image)
    {
        Texture2D texture = new Texture2D((int)image.Width, (int)image.Height);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                texture.SetPixel(x, (int)image.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        SteamFriendsManager.OnFriendsInviteButtonToggle -= (bool b) => _inviteButton.gameObject.SetActive(b);
    }
}