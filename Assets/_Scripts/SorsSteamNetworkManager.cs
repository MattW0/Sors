using System;
using System.Collections;
using UnityEngine;
using Mirror;
using Steamworks;

public class SorsSteamNetworkManager : NetworkManager
{
    private string _playerName;
    private GameOptions _gameOptions;
    public static event Action<GameOptions> OnAllPlayersReady;

    internal void StartMirror(SteamId hostId, string playerName)
    {
        if (NetworkClient.active) return;

        _playerName = playerName;
        if (hostId != SteamClient.SteamId) {
            StartClient(new Uri($"steam://{hostId}"));
            return;
        }

        _gameOptions = GameOptionsMenu.gameOptions;
        StartHost();
    }

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreateCharacter);
    }

    public override void OnStartHost()
    {
        // Currently opponent entity hull that can be targeted
        if(_gameOptions.SinglePlayer){
            var opponent = CreatePlayerObject("Opponent");
            opponent.GetComponent<PlayerManager>().isAI = true;
        }

        StartCoroutine(WaitForAllClients());
    }

    private IEnumerator WaitForAllClients()
    {
        var numPlayers = _gameOptions.SinglePlayer ? 1 : 2;
        while (NetworkServer.connections.Count < numPlayers){
            print("Waiting for opponent...");
            yield return new WaitForSeconds(SorsTimings.wait);
        }

        yield return new WaitForSeconds(SorsTimings.wait);

        OnAllPlayersReady?.Invoke(_gameOptions);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        CreatePlayerMessage playerMessage = new CreatePlayerMessage { name = _playerName };
        NetworkClient.Send(playerMessage);
    }

    private void OnCreateCharacter(NetworkConnectionToClient conn, CreatePlayerMessage message)
    {
        var playerObject = CreatePlayerObject(message.name);
        // call this to use this gameobject as the primary controller
        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    private GameObject CreatePlayerObject(string playerName)
    {
        print($"Creating player {playerName}");
        GameObject playerObject = Instantiate(playerPrefab);

        // spawn player object on server and all clients
        NetworkServer.Spawn(playerObject);

        playerObject.name = playerName;
        
        return playerObject;
    }
}
