using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Mirror.Examples.Basic;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;

    [Header("Game State")]
    public PlayerCards Cards { get ; private set; }
    public List<PrevailOption> _chosenPrevailOptions = new();
    public bool PlayerIsChoosingTarget { get; private set; }
    private PlayerResources _resources;
    private BattleZoneEntity _entity;
    public static event Action<int> OnLocalCashUpdate;
    public static event Action<BattleZoneEntity> OnPlayerChooseEntityTarget;

    public PlayerTurnContext TurnContext { get; private set; } = new();

    #region Stats

    [Header("Game Stats")]
    public int ID { get; private set; }
    [SyncVar(hook="SetPlayerName"), SerializeField] private string _playerName;
    public string PlayerName { get => _playerName; set => _playerName = value; }

    [SyncVar(hook="SetHealth"), SerializeField] private int _health;
    public int Health { get => _health; set => _health = value; }

    [SyncVar(hook="SetScore"), SerializeField] private int _score;
    public int Score { get => _score; set => _score = value; }

    [Header("Turn Stats")]
    [SyncVar(hook="SetBuys"), SerializeField] private int _buys;
    public int Buys { get => _buys; set => _buys = value; }
    
    [SyncVar(hook="SetPlays"), SerializeField] private int _plays;
    public int Plays { get => _plays; set => _plays = value; }

    [SyncVar(hook="SetPrevails"), SerializeField] private int _prevails;
    public int Prevails { get => _prevails; set => _prevails = value; }
    [SyncVar(hook="SetCash"), SerializeField] private int _cash;
    public int Cash { get => _cash; set => _cash = value; }

    public void SetCash(int oldValue, int value)
    {
        if (isOwned) LocalCash = value;
        else _resources.opponentResourcesUI.SetCash(value);
    }

    [SerializeField] private int _localCash;
    public int LocalCash { 
        get => _localCash; 
        set {
            _localCash = value;
            _resources.playerResourcesUI.SetCash(value);
            OnLocalCashUpdate?.Invoke(value);
        }
    }

    #endregion Stats

    #region GameSetup

    private void Awake()
    {
        Cards = GetComponent<PlayerCards>();
        _resources = GameObject.Find("Clients").GetComponent<PlayerResources>();
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
        var entity = GetComponent<BattleZoneEntity>();
        _resources.SetPlayerEntity(isOwned, entity, PlayerName);
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
        // print(PlayerName + " has prevail options: " + options.Count);
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

    #region Utils
    // SyncVar hooks referenced by name, they are used!
    private void SetPlayerName(string oldValue, string newValue) => _resources.UISetPlayerName(isOwned, newValue);
    private void SetHealth(int oldValue, int newValue) => _resources.UISetHealth(isOwned, newValue);
    private void SetScore(int oldValue, int newValue) => _resources.UISetScore(isOwned, newValue);
    private void SetBuys(int oldValue, int newValue) => _resources.UISetBuys(isOwned, newValue);
    private void SetPlays(int oldValue, int newValue) => _resources.UISetPlays(isOwned, newValue);
    private void SetPrevails(int oldValue, int newValue) => _resources.UISetPrevails(isOwned, newValue);
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