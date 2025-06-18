using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;

    [Header("Game State")]
    public PlayerCards Cards { get ; private set; }
    public List<PrevailOption> _chosenPrevailOptions = new();
    public bool PlayerIsChoosingTarget { get; private set; }
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    private BattleZoneEntity _entity;
    public static event Action<int> OnLocalCashUpdate;
    public static event Action<BattleZoneEntity> OnPlayerChooseEntityTarget;

    public PlayerTurnContext TurnContext { get; private set; } = new();

    #region Stats

    [Header("Game Stats")]
    public int ID { get; private set; }
    [SyncVar(hook="UISetPlayerName"), SerializeField] private string _playerName;
    public string PlayerName { get => _playerName; set => _playerName = value; }

    [SyncVar(hook="UISetHealth"), SerializeField] private int _health;
    public int Health { get => _health; set => _health = value; }

    [SyncVar(hook="UISetScore"), SerializeField] private int _score;
    public int Score { get => _score; set => _score = value; }

    [Header("Turn Stats")]
    [SyncVar(hook="UISetBuys"), SerializeField] private int _buys;
    public int Buys { get => _buys; set => _buys = value; }
    
    [SyncVar(hook="UISetPlays"), SerializeField] private int _plays;
    public int Plays { get => _plays; set => _plays = value; }

    [SyncVar(hook="UISetPrevails"), SerializeField] private int _prevails;
    public int Prevails { get => _prevails; set => _prevails = value; }
    [SyncVar(hook="SetCash"), SerializeField] private int _cash;
    public int Cash { get => _cash; set => _cash = value; }

    private void SetCash(int oldValue, int value)
    {
        if (isOwned) LocalCash = value;
        else _opponentUI.SetCash(value);
    }

    [SerializeField] private int _localCash;
    public int LocalCash { 
        get => _localCash; 
        set {
            print("Set local cash");
            _localCash = value;
            _playerUI.SetCash(value);
            OnLocalCashUpdate?.Invoke(value);
        }
    }

    #endregion Stats

    #region GameSetup

    private void Awake()
    {
        Cards = GetComponent<PlayerCards>();
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
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
        TurnContext.PhaseChoices = phases;
        _turnManager.PlayerIsReady(this);
    }

    [Command]
    public void CmdPrevailSelection(List<PrevailOption> options)
    {
        TurnContext.PrevailOptions = options;
        _turnManager.PlayerIsReady(this);
    }

    [Command]
    internal void CmdConfirmSelection(List<int> selectedCardIds)
    {
        TurnContext.SelectedCardIds = selectedCardIds;
        _turnManager.PlayerIsReady(this);
    }

    [Client]
    public void ConfirmPayment(CardSelection choice, InteractionType type)
    {
        LocalCash -= choice.cost;
        CmdConfirmPayment(choice.cardInfo.Value, LocalCash, choice.marketIndex, type);
    }

    [Command]
    private void CmdConfirmPayment(CardInfo cardInfo, int cashBuffer, int marketIndex, InteractionType type)
    {
        TurnContext.SelectedCard = cardInfo;
        TurnContext.CashBuffer = cashBuffer;

        if (type == InteractionType.Buy) _turnManager.PlayerConfirmBuy(this, marketIndex);
        else if (type == InteractionType.Play) _turnManager.PlayerConfirmPlay(this);
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

    // SyncVar hooks referenced by name, they are used!
    private void UISetPlayerName(string oldValue, string newValue)
    {
        if (isOwned) _playerUI.SetName(newValue);
        else _opponentUI.SetName(newValue);
    }
    
    private void UISetHealth(int oldValue, int newValue)
    {
        if (isOwned) _playerUI.SetHealth(newValue);
        else _opponentUI.SetHealth(newValue);
    }

    private void UISetScore(int oldValue, int newValue)
    {
        print("Setting score from " + oldValue + " to " + newValue);
        if (isOwned) _playerUI.SetScore(newValue);
        else _opponentUI.SetScore(newValue);
    }

    private void UISetBuys(int oldValue, int newValue)
    {
        if (isOwned) _playerUI.SetBuys(newValue);
        else _opponentUI.SetBuys(newValue);
    }

    private void UISetPlays(int oldValue, int newValue)
    {
        if (isOwned) _playerUI.SetPlays(newValue);
        else _opponentUI.SetPlays(newValue);
    }

    private void UISetPrevails(int oldValue, int newValue)
    {
        if (isOwned) _playerUI.SetPrevails(newValue);
        else _opponentUI.SetPrevails(newValue);
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