using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private MenuNetwork _menuNetwork;
    
    public void OnHostClick()
    {
        _menuNetwork.PlayerJoins(true);
    }

    public void OnClientClick()
    {
        _menuNetwork.PlayerJoins(false);
    }
}
