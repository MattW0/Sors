using System;
using System.Collections;
using UnityEngine;
using Mirror;
using Cysharp.Threading.Tasks;

namespace Sors.Lan
{

    public class SorsNetworkManager : NetworkManager
    {
        private string _playerName;
        private GameOptions _gameOptions;
        public static event Action<GameOptions> OnAllPlayersReady;

        public override void Awake(){
            base.Awake();
            GameOptionsMenu.OnUpdateNetworkAddress += UpdateNetworkAddress;
        }

        public void PlayerJoins(string playerName, bool isHost)
        {
            if (NetworkClient.active) return;

            _playerName = playerName;
            if (isHost) StartHost();
            else StartClient();
        }

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreateCharacter);
        }

        public override void OnStartHost()
        {
            _gameOptions = GameOptionsMenu.gameOptions;

            // Currently opponent entity hull that can be targeted
            if(_gameOptions.SinglePlayer){
                var opponent = CreatePlayerObject("Opponent");
                opponent.GetComponent<PlayerManager>().isAI = true;
            }

            WaitForAllClients().Forget();
        }

        private async UniTaskVoid WaitForAllClients()
        {
            var numPlayers = _gameOptions.SinglePlayer ? 1 : 2;
            while (NetworkServer.connections.Count < numPlayers){
                print("Waiting for opponent...");
                await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.waitSeconds));
            }

            await UniTask.Delay(TimeSpan.FromSeconds(SorsTimings.waitSeconds));
            OnAllPlayersReady?.Invoke(_gameOptions);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            
            CreatePlayerMessage playerMessage = new CreatePlayerMessage { name = _playerName};
            NetworkClient.Send(playerMessage);
        }

        void OnCreateCharacter(NetworkConnectionToClient conn, CreatePlayerMessage message)
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

        private void UpdateNetworkAddress(string address) => networkAddress = address;

        public override void OnDestroy()
        {
            GameOptionsMenu.OnUpdateNetworkAddress -= UpdateNetworkAddress;
        }
    }
}