using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    private NetworkManager _manager;
    private string _playerNameBuffer;
    private GameOptions _gameOptions;
    public static event Action<GameOptions> OnAllPlayersReady;

    public override void Awake(){
        base.Awake();
        _manager = GetComponent<NetworkManager>();
        GameOptionsMenu.OnUpdateNetworkAddress += UpdateNetworkAddress;
    }

    public override void OnStartServer(){
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreateCharacter);
    }

    public override void OnStartHost(){
        base.OnStartHost();

        _gameOptions = GameOptionsMenu.gameOptions;
        StartCoroutine(WaitingForPlayers());
    }

    private IEnumerator WaitingForPlayers(){

        var numPlayers = _gameOptions.SinglePlayer ? 1 : 2;
        while (NetworkServer.connections.Count < numPlayers){
            print("Waiting for opponent...");
            yield return new WaitForSeconds(SorsTimings.wait);
        }

        yield return new WaitForSeconds(SorsTimings.wait);

        // Currently opponent entity hull that can be targeted
        if(_gameOptions.SinglePlayer){
            var opponent = CreatePlayerObject("Opponent");
            opponent.GetComponent<PlayerManager>().isAI = true;
        }

        OnAllPlayersReady?.Invoke(_gameOptions);
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
        print($"Creating player {playerName}...");
        GameObject playerObject = Instantiate(playerPrefab);

        // spawn player object on server and all clients
        NetworkServer.Spawn(playerObject);

        playerObject.name = playerName;
        
        return playerObject;
    }

    private void UpdateNetworkAddress(string address) => _manager.networkAddress = address;

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameOptionsMenu.OnUpdateNetworkAddress -= UpdateNetworkAddress;
    }
}


