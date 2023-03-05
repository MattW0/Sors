using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    private string[] _networkAddresses = new string[2] {"localhost", "192.168.1.170"};
    private static int _numberPlayersRequired = 2;
    private static int _numberPhasesToChoose = 2;
    private NetworkManager _manager;
    private string _playerNameBuffer;
    public static event Action<int, int> OnAllPlayersReady;

    public override void Awake(){
        base.Awake();
        _manager = GetComponent<NetworkManager>();
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
        while (NetworkServer.connections.Count < _numberPlayersRequired){
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame(){
        yield return new WaitForSeconds(0.5f);
        OnAllPlayersReady?.Invoke(_numberPlayersRequired, _numberPhasesToChoose);
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

    public static void SetNumberPlayers(int numberPlayers) => _numberPlayersRequired = numberPlayers + 1;
    public static void SetNumberPhases(int numberPhases) => _numberPhasesToChoose = numberPhases + 1;
    public void SetNetworkAddress(int networkAddressId) => _manager.networkAddress = _networkAddresses[networkAddressId];

    #endregion
}


