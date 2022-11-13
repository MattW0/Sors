using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MenuNetwork : MonoBehaviour
{
    private NetworkManager _manager;

    private void Awake()
    {
        _manager = GetComponent<NetworkManager>();
    }

    public void PlayerJoins(bool host)
    {
        if (NetworkClient.active) return;
        
        // Server + Client
        if (host) {
            _manager.StartHost();
            print("Starting host");
        }
        // Client
        else {
            _manager.StartClient();
            print("Starting client");
        }
    }
}
