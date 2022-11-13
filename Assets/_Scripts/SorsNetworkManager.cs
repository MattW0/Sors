using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SorsNetworkManager : NetworkManager
{
    private GameManager _gameManager;
    private PlayerManager _host;
    private int _numberPlayersRequired = 2;

    public static new SorsNetworkManager singleton { get; private set; }

    public override void OnStartServer()
    {
        base.OnStartServer();
        networkAddress = "192.168.1.1";
        // _gameManager = GameManager.Instance;
        // _numberPlayersRequired = _gameManager.debug ? 1 : _gameManager.numberPlayers;

        StartCoroutine(WaitingForPlayers());
    }

    private IEnumerator WaitingForPlayers(){
        while (NetworkServer.connections.Count < _numberPlayersRequired)
        {
            Debug.Log("Waiting for players...");
            yield return new WaitForSeconds(1);
        }
        
        GameManager.Instance.GameSetup();
        yield return null;
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        print("Client connected");

        // Disable Mirror HUD
        // gameObject.GetComponent<NetworkManagerHUD>().enabled = false;
    }

    #region Unchanged Overrides
    public override void OnValidate()
    {
        base.OnValidate();
    }
    public override void Awake()
    {
        base.Awake();
    }
    public override void Start()
    {
        singleton = this;
        base.Start();
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion
}
