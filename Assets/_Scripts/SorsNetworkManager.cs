using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    [SerializeField] private int _numberPlayersRequired = 2;
    public static event Action<int> OnAllPlayersReady;
    private NetworkManager _manager;
    private string _playerNameBuffer;

    public override void Awake()
    {
        base.Awake();
        _manager = GetComponent<NetworkManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // networkAddress = "192.168.1.161";

        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreateCharacter);
        StartCoroutine(WaitingForPlayers());
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        print("Host Started");
    }

    private IEnumerator WaitingForPlayers(){
        while (NetworkServer.connections.Count < _numberPlayersRequired)
        {
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        
        OnAllPlayersReady?.Invoke(_numberPlayersRequired);
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
}
