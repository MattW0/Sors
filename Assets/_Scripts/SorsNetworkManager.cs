using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    private GameManager _gameManager;
    private PlayerManager _host;
    private int _numberPlayersRequired;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _gameManager = GameManager.Instance;
        _numberPlayersRequired = _gameManager.debug ? 1 : 2;

        StartCoroutine(WaitingForPlayers());
    }

    private IEnumerator WaitingForPlayers(){
        while (NetworkServer.connections.Count < _numberPlayersRequired)
        {
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        _gameManager.GameSetup();
        yield return null;
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Client connected");

        // Disable Mirror HUD
        gameObject.GetComponent<NetworkManagerHUD>().enabled = false;
    }
}
