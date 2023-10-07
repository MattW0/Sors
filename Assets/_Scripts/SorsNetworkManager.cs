using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    private NetworkManager _manager;
    private string _playerNameBuffer;
    private string[] _networkAddresses = new string[2] {"localhost", "192.168.1.170"};
    public static GameOptions gameOptions = new GameOptions(2, 2, false, false, "192.168.1.170", "");
    public static event Action<GameOptions> OnAllPlayersReady;

    public override void Awake(){
        base.Awake();
        _manager = GetComponent<NetworkManager>();
        InputField.OnNetworkAdressUpdate += UpdateNetworkAddress;
    }

    public override void OnStartServer(){
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreateCharacter);
    }

    public override void OnStartHost(){
        base.OnStartHost();
        print("Host Started");
        StartCoroutine(WaitingForPlayers());
    }

    private IEnumerator WaitingForPlayers(){
        while (NetworkServer.connections.Count < gameOptions.NumberPlayers){
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame(){
        yield return new WaitForSeconds(0.5f);

        OnAllPlayersReady?.Invoke(gameOptions);
        yield return null;
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        if(string.IsNullOrEmpty(_playerNameBuffer)) print("No player name recieved!");
        
        CreatePlayerMessage playerMessage = new CreatePlayerMessage { name = _playerNameBuffer};

        _playerNameBuffer = null;
        NetworkClient.Send(playerMessage);
    }

    void OnCreateCharacter(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        print("Creating player " + message.name);
        GameObject playerObject = Instantiate(playerPrefab);

        PlayerManager player = playerObject.GetComponent<PlayerManager>();
        player.PlayerName = message.name;

        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    public void PlayerWantsToJoin(string playerName, bool host)
    {
        if (NetworkClient.active) return;

        PlayerJoins(playerName, host);
    }

    private void PlayerJoins(string playerName, bool isHost)
    {
        _playerNameBuffer = playerName;
        if (isHost) _manager.StartHost();
        else _manager.StartClient();
    }

    #region Host Options
    public static void SetNumberPlayers(int numberPlayers) => gameOptions.NumberPlayers = numberPlayers + 1;
    public static void SetNumberPhases(int numberPhases) => gameOptions.NumberPhases = numberPhases + 1;
    public static void SetFullHand(bool drawAll) => gameOptions.FullHand = drawAll;
    public static void SetSpawnimations(bool b) => gameOptions.CardSpawnAnimations = b;
    public static void SetStateFile(string stateFile) => gameOptions.StateFile = stateFile;
    private void UpdateNetworkAddress(string networkAddress){
        _manager.networkAddress = networkAddress;
        gameOptions.NetworkAddress = networkAddress;
    }

    #endregion

    public override void OnDestroy() {
        base.OnDestroy();
        InputField.OnNetworkAdressUpdate -= UpdateNetworkAddress;
    }
}


