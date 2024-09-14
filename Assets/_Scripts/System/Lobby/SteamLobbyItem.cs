using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using UnityEngine.UI;
using TMPro;

public class SteamLobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyId;
    [SerializeField] private TMP_Text _lobbyName;
    [SerializeField] private TMP_Text _lobbyOwner;
    [SerializeField] private Button _joinButton;

    public void SetLobby(Lobby lobby, SteamLobbiesManager steamLobbiesManager)
    {
        _lobbyId.text = lobby.Id.ToString();
        _lobbyName.text = lobby.GetData("name");
        _lobbyOwner.text = lobby.Owner.Name;

        _joinButton.onClick.AddListener(() => steamLobbiesManager.JoinLobby(lobby.Id));
    }
}
