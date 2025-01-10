using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour {
    
    public static GameManager Instance { get; private set; }
    private INetworkObjectSpawner _cardSpawner;
    private TurnManager _turnManager;
    private UIManager _uiManager;
    private Market _market;
    private GameOptions _gameOptions;
    public static event Action<GameOptions> OnGameStart;

    [Header("Game state")]
    public int turnNumber;
    public Dictionary<NetworkIdentity, PlayerManager> players = new();
    private List<PlayerManager> _loosingPlayers = new();
    private List<PlayerManager> _winningPlayers = new();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;

        Sors.Lan.SorsNetworkManager.OnAllPlayersReady += GameSetup;
        SorsSteamNetworkManager.OnAllPlayersReady += GameSetup;

        _cardSpawner = GetComponent<INetworkObjectSpawner>();
    }

    #region Setup
    
    public void GameSetup(GameOptions options)
    {
        _turnManager = TurnManager.Instance;
        _market = Market.Instance;
        _uiManager = UIManager.Instance;

        print(" --- Game starting --- \n" + options.ToString());
        _gameOptions = options;
        // isSinglePlayer = options.SinglePlayer;

        InitPlayers();

        if(string.IsNullOrWhiteSpace(options.StateFile)){
            // Normal game setup
            _market.RpcInitializeMarket();
            foreach (var player in players.Values) SpawnPlayerDeck(player);
            OnGameStart?.Invoke(_gameOptions);
        } else {
            // Start game from state file
            gameObject.GetComponent<GameStateLoader>().LoadGameState(options.StateFile);
        }
    }

    private void InitPlayers()
    {
        var playerManagers = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        foreach (var player in playerManagers)
        {
            var networkId = player.GetComponent<NetworkIdentity>();
            player.RpcInitPlayer((int) networkId.netId);

            // TODO: Most things break if we actually want an AI opponent
            if(!player.isAI) players.Add(networkId, player);
            
            // Player stats
            player.PlayerName = player.gameObject.name; // Object name is set after instantiation in NetworkManager
            player.Health = _gameOptions.startHealth;
            player.Score = 0;
            
            // Turn Stats
            player.Cash = 0;
            player.Buys = 0;
            player.Plays = 0;
            player.Prevails = 0;
        }

        if(_gameOptions.SkipCardSpawnAnimations) {
            // Needs to be done on all clients
            playerManagers[0].RpcSkipCardSpawnAnimations();
            // And on server
            SkipCardSpawnAnimations();
        }
    }

    private void SpawnPlayerDeck(PlayerManager player)
    {
        List<GameObject> startingDeck = new();

        // Only paper money currently
        var startMoney = _market.GetStartMoneyCard();
        for (var i = 0; i < _gameOptions.initialDeckSize - _gameOptions.initialEntities; i++){
            var scriptableCard = startMoney;
            startingDeck.Add(_cardSpawner.SpawnCard(player, scriptableCard, CardLocation.Deck));
        }

        var startEntities = _market.GetStartEntities();
        for (var i = 0; i < _gameOptions.initialEntities; i++){
            var scriptableCard = startEntities[i];
            startingDeck.Add(_cardSpawner.SpawnCard(player, scriptableCard, CardLocation.Deck));
        }

        player.Cards.RpcShowSpawnedCards(startingDeck, CardLocation.Deck, false);
    }

    #endregion

    #region Spawning
    public void PlayerGainCard(PlayerManager player, CardInfo card, CardLocation destination) => _cardSpawner.PlayerGainCard(player, card, destination);
    public GameObject SpawnCard(PlayerManager player, ScriptableCard card, CardLocation destination) => _cardSpawner.SpawnCard(player, card, destination);
    public void PlayerGainCurse(PlayerManager player) => _cardSpawner.PlayerGainCurse(player);
    public BattleZoneEntity SpawnFieldEntity(PlayerManager owner, CardInfo cardInfo) => _cardSpawner.SpawnFieldEntity(owner, cardInfo);

    #endregion

    #region Ending
    internal void PlayerIsDead(PlayerManager player){
        if(_loosingPlayers.Contains(player)) return;
        _loosingPlayers.Add(player);
    }
    internal void PlayerHasWinScore(PlayerManager player) {
        if(_winningPlayers.Contains(player)) return;
        _winningPlayers.Add(player);
    }
    internal void EndGame()
    {
        // TODO: Define and implement properly

        foreach (var player in players.Values){
            _uiManager.RpcSetPlayerScore(player, player.Health, player.Score);
        }

        PlayerManager winner = null;
        if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 0) {} // Should never happen
        else if(_winningPlayers.Count == 1 && _loosingPlayers.Count == 0){
            winner = _winningPlayers[0];
        } else if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 1){
            winner = _turnManager.GetOpponentPlayer(_loosingPlayers[0]);
        } else if (_winningPlayers.Count == 1 && _loosingPlayers.Count == 1){
            if (_winningPlayers[0] != _loosingPlayers[0]) winner = _winningPlayers[0];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 2 && _loosingPlayers.Count == 0){
            if (_winningPlayers[0].Score > _winningPlayers[1].Score) winner = _winningPlayers[0];
            else if (_winningPlayers[0].Score < _winningPlayers[1].Score) winner = _winningPlayers[1];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 0 && _loosingPlayers.Count == 2){
            if (_loosingPlayers[0].Health > _loosingPlayers[1].Health) winner =_loosingPlayers[0];
            else if (_loosingPlayers[0].Health < _loosingPlayers[1].Health) winner = _loosingPlayers[1];
            // else _endScreen.RpcGameIsDraw();
        } else if (_winningPlayers.Count == 2 && _loosingPlayers.Count == 1){
            if(_winningPlayers[0] == _loosingPlayers[0]) winner = _winningPlayers[1];
            else winner = _winningPlayers[0];
        } else if (_winningPlayers.Count == 1 && _loosingPlayers.Count == 2){
            if(_winningPlayers[0] == _loosingPlayers[0]) winner = _loosingPlayers[0];
            else winner = _winningPlayers[1];
        } else {
            if (_winningPlayers[0].Score > _winningPlayers[1].Score) winner = _winningPlayers[0];
            else if (_winningPlayers[0].Score < _winningPlayers[1].Score) winner = _winningPlayers[1];
            else if (_loosingPlayers[0].Health > _loosingPlayers[1].Health) winner =_loosingPlayers[0];
            else if (_loosingPlayers[0].Health < _loosingPlayers[1].Health) winner = _loosingPlayers[1];
        }

        if(winner) _uiManager.RpcSetGameWinner(winner);
        else _uiManager.RpcSetDraw();
    }

    #endregion

    #region Utils
    public void StartGame() => OnGameStart?.Invoke(_gameOptions);

    [Server]
    private void SkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();

    public PlayerManager GetOpponent(PlayerManager player)
    {
        return players.Values.FirstOrDefault(p => p != player);
    }
    #endregion

    private void OnDestroy()
    {
        SorsSteamNetworkManager.OnAllPlayersReady -= GameSetup;
        Sors.Lan.SorsNetworkManager.OnAllPlayersReady -= GameSetup;
    }
}
