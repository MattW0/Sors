using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private SorsNetworkManager _networkManager;
    [SerializeField] private TMP_InputField _nameInput;
    private string _playerName;

    public void OnHostClick()
    {
        if (string.IsNullOrEmpty(_nameInput.text)) _playerName = "Host";
        else _playerName = _nameInput.text;

        _networkManager.PlayerWantsToJoin(_playerName, true);
    }

    public void OnClientClick()
    {
        if (string.IsNullOrEmpty(_nameInput.text)) _playerName = "Client";
        else _playerName = _nameInput.text;

        _networkManager.PlayerWantsToJoin(_playerName, false);
    }
}
