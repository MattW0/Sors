using UnityEngine;
using TMPro;
using System.Net;
using System.Linq;
using UnityEngine.UI;
using Sors.Lan;

namespace Sors.Lan
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private SorsNetworkManager _networkManager;
        [SerializeField] private Button _hostGame;
        [SerializeField] private Button _joinGame;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Text _playerNetworkAddressText;
        private string _playerName;
        
        private void Start()
        {
            _hostGame.onClick.AddListener(StartHost);
            _joinGame.onClick.AddListener(StartClient);
            
            _playerNetworkAddressText.text = "Player IPv4 : " + GetLocalIPv4();
        }

        private void StartHost()
        {
            if (string.IsNullOrEmpty(_nameInput.text)) _playerName = "Host";
            else _playerName = _nameInput.text;

            _networkManager.PlayerJoins(_playerName, true);
        }

        private void StartClient()
        {
            if (string.IsNullOrEmpty(_nameInput.text)) _playerName = "Client";
            else _playerName = _nameInput.text;

            _networkManager.PlayerJoins(_playerName, false);
        }

        private string GetLocalIPv4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
        }
    }
}
