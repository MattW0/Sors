using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private SorsNetworkManager _networkManager;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_Text _playerNetworkAddressText;
    private string _playerName;
    private void Start() => _playerNetworkAddressText.text = "Player IPv4 : " + _networkManager.GetLocalIPv4(); 

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
