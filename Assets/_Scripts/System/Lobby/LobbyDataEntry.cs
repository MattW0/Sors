using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyDataEntry : MonoBehaviour
{
    private CSteamID _lobbyId;
    public string lobbyName;
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private Button _joinLobbyButton;

    // private void Start()
    // {
    //     _joinLobbyButton.onClick.AddListener(JoinLobby);
    // }

    public void SetLobbyData(CSteamID lobbyId, string lobbyName)
    {
        _lobbyId = lobbyId;
        if (lobbyName == "" || lobbyName == null){
            lobbyName = "Empty";
        } else {
            _lobbyNameText.text = lobbyName;
        }
    }

    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobby(_lobbyId);
    }
}
