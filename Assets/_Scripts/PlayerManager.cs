using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;
using System.Linq;
using Cysharp.Threading.Tasks;
using SorsGameState;
using CardDecoder;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;

    [Header("Game State")]
    private CardCollection _cards;
    public CardCollection Cards { get => _cards; set => _cards = value; }
    public List<PrevailOption> _chosenPrevailOptions = new();
    public bool PlayerIsChoosingTarget { get; private set; }
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    private BattleZoneEntity _entity;
    public static event Action<PlayerManager, int> OnCashChanged;
    public static event Action<BattleZoneEntity> OnPlayerChooseEntityTarget;

    #region Stats

    [Header("Game Stats")]
    [SyncVar, SerializeField] private string playerName;
    public string PlayerName
    {
        get => playerName;
        set => SetPlayerName(value);
    }

    [SerializeField] private int _health;
    public int Health
    {
        get => _health;
        set => SetHealthValue(value);
    }

    [SerializeField] private int _score;
    public int Score
    {
        get => _score;
        set => SetScoreValue(value);
    }

    [Header("Turn Stats")]
    [SyncVar(hook="UISetMoneyValue"), SerializeField] private int _cash;
    public int Cash
    {
        get => _cash;
        set {
            _cash = value;
            OnCashChanged?.Invoke(this, value);
            SetMoneyValue(value);
        }
    }

    [SyncVar, SerializeField] private int _buys = 1;
    public int Buys
    {
        get => _buys;
        set => SetBuyValue(value);
    }
    
    [SyncVar, SerializeField] private int _plays = 1;
    public int Plays
    {
        get => _plays;
        set => SetPlayValue(value);
    }

    [SyncVar, SerializeField] private int _prevails;
    public int Prevails
    {
        get => _prevails;
        set => SetPrevailValue(value);
    }

    public int ID { get; private set; }

    #endregion Stats

    #region GameSetup

    private void Awake()
    {
        _cards = GetComponent<CardCollection>();
    }

    [ClientRpc]
    public void RpcInitPlayer(int playerId)
    {
        ID = playerId;
        print("Player ID: " + ID);
        EntityAndUISetup();

        if (!isServer) return;
        _turnManager = TurnManager.Instance;
        _entity = GetComponent<BattleZoneEntity>();
    }

    private void EntityAndUISetup()
    {
        // TODO: Check if playerUI logic needs to be here or if it can be done in PlayerUI class
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
        
        var entity = GetComponent<BattleZoneEntity>();
        if(isOwned) {
            entity.SetPlayer(PlayerName, _playerUI);
            // Child 0 is the player stats BG
            _playerUI.SetEntity(entity, _playerUI.transform.GetChild(0).position);
        } else {
            entity.SetPlayer(PlayerName, _opponentUI);
            // Child 0 is the player stats BG
            _opponentUI.SetEntity(entity, _opponentUI.transform.GetChild(0).position);
        }
    }
    #endregion GameSetup

    #region Turn Actions

    [Command]
    public void CmdPhaseSelection(List<TurnState> phases)
    {
        print($"    - {PlayerName} selection: {string.Join(", ", phases)}");
        _turnManager.PlayerSelectedPhases(this, phases.ToArray());
    }

    [Command]
    public void CmdDiscardSelection(List<CardStats> cardsToDiscard)
    {
        _turnManager.PlayerSelectedDiscardCards(this, cardsToDiscard);
    }

    [Command]
    public void CmdConfirmBuy(MarketSelection card) => _turnManager.PlayerConfirmBuy(this, card);

    [Command]
    public void CmdConfirmPlay(List<CardStats> cards)
    {
        // TODO: Make playing multiple cards possible ?
        foreach(var card in cards) _turnManager.PlayerPlaysCard(this, card);
        
        Cards.RemoveHandCards(cards, CardLocation.PlayZone);
    }

    [Command]
    public void CmdPrevailSelection(List<PrevailOption> options)
    {
        // Saving local player choice
        _chosenPrevailOptions = options;
        _turnManager.PlayerSelectedPrevailOptions(this, options);
    }

    [Command]
    public void CmdPrevailCardsSelection(List<CardStats> cards)
    {
        _turnManager.PlayerSelectedPrevailCards(this, cards);
    }

    [Command]
    public void CmdSkipInteraction() => _turnManager.PlayerSkipsInteraction(this);

    #endregion TurnActions

    #region Effect Interactions

    [TargetRpc]
    public void TargetPlayerStartChooseTarget()
    {
        // TODO: Need to expand this ?
        // Allows player to click entity and define target in targetArrowHandler (double fail save)
        // possibly must use for multiple targets
        PlayerIsChoosingTarget = true;
    }

    public void PlayerChoosesEntityTarget(BattleZoneEntity target)
    {
        if (isServer) OnPlayerChooseEntityTarget?.Invoke(target); // _abilityQueue.PlayerChoosesAbilityTarget(target);
        else CmdPlayerChoosesTargetEntity(target);

        PlayerIsChoosingTarget = false;
    }

    [Command]
    private void CmdPlayerChoosesTargetEntity(BattleZoneEntity target) => OnPlayerChooseEntityTarget?.Invoke(target); 

    #endregion

    #region Resources UI

    private void SetPlayerName(string name)
    {
        playerName = name;
        RpcUISetPlayerName(name);
    }

    [ClientRpc]
    public void RpcUISetPlayerName(string name)
    {
        playerName = name;
        if (isOwned) _playerUI.SetName(name);
        else _opponentUI.SetName(name);
    }

    [Server]
    private void SetHealthValue(int value)
    {
        _health = value;
        RpcUISetHealthValue(value);
    }

    [ClientRpc]
    private void RpcUISetHealthValue(int value)
    {
        if (isOwned) _playerUI.SetHealth(value);
        else _opponentUI.SetHealth(value);
    }

    [Server]
    private void SetScoreValue(int value)
    {
        _score = value;
        RpcUISetScoreValue(value);
    }

    [ClientRpc]
    private void RpcUISetScoreValue(int value)
    {
        if (isOwned) _playerUI.SetScore(value);
        else _opponentUI.SetScore(value);
    }

    [Server]
    private void SetMoneyValue(int value)
    {
        _cash = value;
        RpcUISetMoneyValue(value);
    }

    [ClientRpc]
    private void RpcUISetMoneyValue(int value)
    {
        if (isOwned) _playerUI.SetCash(value);
        else _opponentUI.SetCash(value);
    }

    private void UISetMoneyValue(int oldValue, int newValue)
    {
        if (isOwned) _playerUI.SetCash(newValue);
        else _opponentUI.SetCash(newValue);
    }

    [Server]
    private void SetBuyValue(int value)
    {
        _buys = value;
        RpcSetBuyValue(value);
    }

    [ClientRpc]
    private void RpcSetBuyValue(int value)
    {
        if (isOwned) _playerUI.SetBuys(value);
        else _opponentUI.SetBuys(value);
    }

    [Server]
    private void SetPlayValue(int value)
    {
        _plays = value;
        RpcSetPlayValue(value);
    }

    [ClientRpc]
    private void RpcSetPlayValue(int value)
    {
        if (isOwned) _playerUI.SetPlays(value);
        else _opponentUI.SetPlays(value);
    }

    [Server]
    private void SetPrevailValue(int value)
    {
        _prevails = value;
        RpcSetPrevailValue(value);
    }

    [ClientRpc]
    private void RpcSetPrevailValue(int value)
    {
        if (isOwned) _playerUI.SetPrevails(value);
        else _opponentUI.SetPrevails(value);
    }
    #endregion UI

    #region Utils

    public static PlayerManager GetLocalPlayer()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    [ClientRpc] public void RpcSkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();
    [Server] internal BattleZoneEntity GetEntity() => _entity;
    [Server] public void ForceEndTurn() => _turnManager.ForceEndTurn();

    public bool Equals(PlayerManager other)
    {
        if (other == null) return false;

        // Optimization for a common success case.
        if (ReferenceEquals(this, other)) return true;

        return connectionToClient == other.connectionToClient;
    }

    #endregion
}