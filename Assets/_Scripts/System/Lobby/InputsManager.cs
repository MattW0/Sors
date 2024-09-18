using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System;
using Michsky.UI.Shift;

public class InputsManager : MonoBehaviour
{
    [SerializeField] private Button _hostGame;
    [SerializeField] private Button _startGame;
    [SerializeField] private Button _leaveLobby;

    // [SerializeField] private Button _searchGame;
    // [SerializeField] private Button _ready;
    [SerializeField] private SteamLobbiesManager _steamLobbiesManager;
    private MainPanelManager _mainPanelManager;

    private void Awake()
    {
        SteamPlayerItem.OnSteamPlayerClicked += PlayerClicked;

        ModalWindow.OnAccept += ModalWindowAccept;
        ModalWindow.OnDecline += ModalWindowDecline;
    }

    private void Start()
    {
        _hostGame.onClick.AddListener(HostGame);
        _startGame.onClick.AddListener(StartGame);
        _leaveLobby.onClick.AddListener(LeaveLobby);

        // _searchGame.onClick.AddListener(SearchGame);
        // _ready.onClick.AddListener(ReadyUp);

        _mainPanelManager = GetComponent<MainPanelManager>();
    }

    private void HostGame()
    {
        _steamLobbiesManager.CreateLobby();
        _mainPanelManager.OpenPanel("Lobby");
    }

    private void PlayerClicked(SteamId id, ButtonAction action)
    {
        print("Player clicked: " + id + ", " + action);    
        if (action == ButtonAction.INVITE) _steamLobbiesManager.InvitePlayer(id);
        else if (action == ButtonAction.JOIN) _steamLobbiesManager.JoinLobby(id);
        else if (action == ButtonAction.KICK) _steamLobbiesManager.KickPlayer(id);
    }

    private void LeaveLobby(){
        _mainPanelManager.OpenPanel("Home");
        _steamLobbiesManager.LeaveLobby();
    } 
    // private void ReadyUp() => _steamLobbiesManager.ReadyUp();
    private void StartGame() => _steamLobbiesManager.StartGame();

    private void ModalWindowAccept(ModalWindowType type)
    {
        if (type == ModalWindowType.EXIT) ExitGame();
        else if (type == ModalWindowType.LOBBY_INVITE) JoinLobbyViaInvite();
    }

    private void ModalWindowDecline(ModalWindowType type)
    {
        if (type == ModalWindowType.LOBBY_INVITE) DeclineLobbyInvite();
    }

    private void JoinLobbyViaInvite() => _steamLobbiesManager.JoinLobbyViaInvite();
    private void DeclineLobbyInvite() 
    {
         print("Declined invite");  // TODO
    }

    private void ExitGame()
        {
            Debug.Log("Exit function does not work in editor mode.");
            Application.Quit();
        }

    private void OnDestroy()
    {
        SteamPlayerItem.OnSteamPlayerClicked -= PlayerClicked;

        ModalWindow.OnAccept -= ModalWindowAccept;
        ModalWindow.OnDecline -= ModalWindowDecline;
    }
}
