using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;

public class SteamNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController _playerObjectController;
    public List<PlayerObjectController> GamePlayers { get; } = new();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        print(SceneManager.GetActiveScene().name); // Debug.Log(SceneManager.GetActiveScene().name
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            var player = Instantiate(_playerObjectController);

            player.connectionId = conn.connectionId;
            player.playerIdNumber = GamePlayers.Count + 1;
            player.playerSteamId = (ulong) SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
        }
    }
}
