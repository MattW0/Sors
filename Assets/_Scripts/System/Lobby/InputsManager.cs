using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Steamworks;

public class InputsManager : MonoBehaviour
{
    [SerializeField] private Button _hostGame;
    // [SerializeField] private Button _searchGame;
    [SerializeField] private Button _leaveLobby;
    [SerializeField] private Button _ready;
    [SerializeField] private Button _startGame;
    [SerializeField] private Button _cardsScene;
    [SerializeField] private SteamLobbiesManager _steamLobbiesManager;

    private void Awake()
    {
        SteamPlayerItem.OnJoin += JoinLobby;
        SteamPlayerItem.OnInvite += InviteFriend;

        ConfirmDialogue.OnAccept += JoinLobbyViaInvite;
        ConfirmDialogue.OnDecline += DeclineLobbyInvite;
    }

    private void Start()
    {
        _hostGame.onClick.AddListener(HostGame);
        // _searchGame.onClick.AddListener(SearchGame);
        _leaveLobby.onClick.AddListener(LeaveLobby);
        // _ready.onClick.AddListener(ReadyUp);
        _startGame.onClick.AddListener(StartGame);
        _cardsScene.onClick.AddListener(LoadCardScene);
    }

    private void HostGame() => _steamLobbiesManager.CreateLobby();
    private void InviteFriend(SteamId id) => _steamLobbiesManager.InviteFriend(id);
    private void JoinLobby(SteamId id) => _steamLobbiesManager.JoinLobby(id);
    private void JoinLobbyViaInvite() => _steamLobbiesManager.JoinLobbyViaInvite();
    private void DeclineLobbyInvite() {
         print("Declined invite");  // TODO
    }
    private void LeaveLobby() => _steamLobbiesManager.LeaveLobby();
    // private void ReadyUp() => _steamLobbiesManager.ReadyUp();
    private void StartGame() => _steamLobbiesManager.StartGame();
    private void LoadCardScene() => SceneManager.LoadScene("Cards");

    private void OnDestroy()
    {
        SteamPlayerItem.OnJoin -= JoinLobby;
        SteamPlayerItem.OnInvite -= InviteFriend;

        ConfirmDialogue.OnAccept -= JoinLobbyViaInvite;
        ConfirmDialogue.OnDecline -= DeclineLobbyInvite;
    }
}
