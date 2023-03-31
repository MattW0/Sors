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
    private GameOptions _gameOptions = new GameOptions(2, 2, false, "localhost");
    public static event Action<int, int, bool> OnAllPlayersReady;

    public override void Awake(){
        base.Awake();
        _manager = GetComponent<NetworkManager>();

        InputField.OnInputFieldChanged += SetNetworkAddress;
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
        while (NetworkServer.connections.Count < GameOptions.NumberPlayers){
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame(){
        yield return new WaitForSeconds(0.5f);
        OnAllPlayersReady?.Invoke(GameOptions.NumberPlayers, 
                                  GameOptions.NumberPhases,
                                  GameOptions.FullHand);
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
    public static void SetNumberPlayers(int numberPlayers) => GameOptions.NumberPlayers = numberPlayers + 1;
    public static void SetNumberPhases(int numberPhases) => GameOptions.NumberPhases = numberPhases + 1;
    public static void SetFullHand(bool drawAll) => GameOptions.FullHand = drawAll;
    public void SetNetworkAddress(string networkAddress) => _manager.networkAddress = networkAddress;
    #endregion

    public override void OnDestroy() {
        base.OnDestroy();
        InputField.OnInputFieldChanged -= SetNetworkAddress;
    }
}

public struct GameOptions{

    public static int NumberPlayers { get; set;}
    public static int NumberPhases { get; set;}
    public static bool FullHand { get; set;}
    public static string NetworkAddress { get; set;}
    public GameOptions(int numPlayers, int numPhases, bool fullHand, string address){
        NumberPlayers = numPlayers;
        NumberPhases = numPhases;
        FullHand = fullHand;
        NetworkAddress = address;
    }
} 


