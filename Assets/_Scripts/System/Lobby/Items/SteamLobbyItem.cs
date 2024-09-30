using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using TMPro;

public class SteamLobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyName;
    [SerializeField] private TMP_Text _lobbyOwner;
    [SerializeField] private Button _joinButton;
    private SteamId _lobbyId;

    public void SetLobby(Lobby lobby, SteamLobbiesManager steamLobbiesManager)
    {
        _lobbyId = lobby.Id;
        _lobbyName.text = lobby.GetData("name");
        _lobbyOwner.text = lobby.Owner.Name;

        _joinButton.onClick.AddListener(() => steamLobbiesManager.JoinLobby(lobby.Id));
    }
}
