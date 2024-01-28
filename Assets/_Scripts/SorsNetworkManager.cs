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
    public static GameOptions gameOptions = new GameOptions(2, 1, true, false, "192.168.1.170", "", 4);
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
            print("Waiting for players...");
            yield return new WaitForSeconds(SorsTimings.wait);
        }

        if(gameOptions.NumberPlayers == 1){
            var opponent = CreatePlayerObject("Opponent");
            opponent.GetComponent<PlayerManager>().isAI = true;
        }
        yield return new WaitForSeconds(SorsTimings.wait);
        OnAllPlayersReady?.Invoke(gameOptions);
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
        var playerObject = CreatePlayerObject(message.name);
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

    private GameObject CreatePlayerObject(string playerName)
    {
        print("Creating player " + playerName);
        GameObject playerObject = Instantiate(playerPrefab);
        NetworkServer.Spawn(playerObject);

        playerObject.name = playerName;
        // playerObject.GetComponent<PlayerManager>().PlayerName = playerName;
        
        return playerObject;
    }

    #region Host Options
    public static void SetNumberPlayers(int numberPlayers) => gameOptions.NumberPlayers = numberPlayers + 1;
    public static void SetNumberPhases(int numberPhases) => gameOptions.NumberPhases = numberPhases + 1;
    public static void SetFullHand(bool drawAll){
        gameOptions.FullHand = drawAll;
        gameOptions.InitialHandSize = drawAll ? 10 : 4;
    } 
    public static void SetSpawnimations(bool b) => gameOptions.SkipCardSpawnAnimations = b;
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


