using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;
using System;

public class PlayerDataEntry : MonoBehaviour
{
    public string playerName;
    public int connectionId;
    public ulong playerSteamId;
    private bool _avatarReceived;

    [SerializeField] public TMP_Text _playerNameText;
    [SerializeField] public RawImage _playerAvatarImage;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    private void Start()
    {
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == playerSteamId)
        {
            _playerAvatarImage.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
    }

    public void SetPlayerData()
    {
        _playerNameText.text = playerName;
        if (_avatarReceived) return;
        GetPlayerIcon();
    }

    private void GetPlayerIcon()
    {
        int imageId = SteamFriends.GetLargeFriendAvatar(new CSteamID(playerSteamId));
        if (imageId == -1) return;
        _playerAvatarImage.texture = GetSteamImageAsTexture(imageId);
    }

    private Texture2D GetSteamImageAsTexture(int m_iImage)
    {
        bool success = SteamUtils.GetImageSize(m_iImage, out uint width, out uint height);
        if (! success) return null;
        
        byte[] image = new byte[width * height * 4];
        success = SteamUtils.GetImageRGBA(m_iImage, image, 32 * 32 * 4);
        if (! success) return null;

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(image);
        texture.Apply();

        _avatarReceived = true;
        return texture;
    }
}
