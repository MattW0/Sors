using System;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

[RequireComponent(typeof(PlayerInterfaceButtons))]
public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    [SerializeField] private Chat _chat;
    [SerializeField] private ActionDescription _actionDescription;
    private PlayerInterfaceButtons _buttons;
    private PlayerManager _player;
    private Dictionary<int, string> _messageOrigin = new() {{0, "Game"}};
    private const int COMPUTER_PLAYER_ID = 1;
    private const string COMPUTER_PLAYER_NAME = "Computer";
    public static event Action<string, string> OnChatMessageReceived;
    private SorsColors _colorPalette;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _buttons = GetComponent<PlayerInterfaceButtons>();
        TurnManager.OnTurnStateChanged += RpcChangeActionDescriptionText;

        _colorPalette = Resources.Load<SorsColors>("Sors Colors");
    }

    [ClientRpc]
    public void RpcPrepare(PlayerManager[] players, int numberPhasesToChoose)
    {
        _actionDescription.NumberPhases = numberPhasesToChoose;
        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) gameObject.GetComponent<PlayerInterfaceButtons>().DisableUtilityButton();
        // else TurnManager.OnPlayerIsReady += RpcLogPlayerAction;

        foreach (var p in players)
        {
            if(p.ID == _player.ID) _messageOrigin.Add(p.ID, p.PlayerName.AddColor(_colorPalette.player));
            else _messageOrigin.Add(p.ID, p.PlayerName.AddColor(_colorPalette.opponent));
        }

        // Single-player (Count < 3 because 'Game' is another origin)
        if (_messageOrigin.Count < 3) _messageOrigin.Add(COMPUTER_PLAYER_ID, COMPUTER_PLAYER_NAME);
        
        _logger.StartGame(_messageOrigin.Values.ToArray());
    }

    [ClientRpc]
    private void RpcChangeActionDescriptionText(TurnState state) => _actionDescription.ChangeActionDescriptionText(state);

    [ClientRpc]
    public void RpcBeginTurn(int turnNumber)
    {
        _actionDescription.StartTurn(turnNumber);
        _logger.TurnStart(_messageOrigin[0], turnNumber);   
    }
    // [ClientRpc]
    // public void RpcUndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    // [TargetRpc]
    // public void TargetUndoButtonEnabled(NetworkConnection conn, bool b) => _buttons.UndoButtonEnabled(b);
    // public void UndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    // Only used for undo on playing money cards
    // TODO: attackers, blockers choices
    public void Undo() => _player.Cards.CmdUndoPlayMoney();
    public void ForceEndTurn() => _player.ForceEndTurn();

    #region Log
    [ClientRpc] public void RpcLog(int winner) => _logger.EndGame(_messageOrigin[winner]);
    [ClientRpc] public void RpcLog(List<TurnState> phases) => _logger.PhasesToPlay(_messageOrigin[0], phases);
    [ClientRpc] public void RpcLog(TurnState newState) => _logger.PhaseChange(_messageOrigin[0], newState);
    [ClientRpc] public void RpcLog(int playerId, int number) => _logger.PlayerDrawsCards(_messageOrigin[playerId], number);
    [ClientRpc] public void RpcLog(int playerId, string clash) => _logger.Log(clash, _messageOrigin[playerId], LogType.CombatClash);
    [ClientRpc] public void RpcLog(int playerId, List<CardStats> cards) => _logger.PlayerDiscardsCards(_messageOrigin[playerId], cards.Select(c => c.cardInfo.title).ToList());
    [ClientRpc] public void RpcLog(int playerId, string cardName, int cost, LogType type) => _logger.PlayerSpendsCash(_messageOrigin[playerId], cardName, cost, type);
    [ClientRpc] public void RpcLog(int playerId, string sourceTitle, string targetTitle, LogType type) => _logger.PlayerTargeting(_messageOrigin[playerId], sourceTitle, targetTitle, type);

    #endregion

    #region Chat

    [Client]
    public void Send(string message) => CmdSendMessage(isServer, message);

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(bool isHost, string message) {
        RpcHandleMessage(isHost ? _messageOrigin[1] : _messageOrigin[2], message);
    }

    [ClientRpc]
    private void RpcHandleMessage(string originator, string message) => OnChatMessageReceived?.Invoke(originator, message);
    public void ToggleLogChat()
    {
        _chat.ToggleVisible();
        _logger.ToggleVisible();
    }

    #endregion

    private void OnDestroy()
    {
        // TurnManager.OnPlayerIsReady -= RpcLogPlayerAction;
        TurnManager.OnTurnStateChanged -= RpcChangeActionDescriptionText;
    }
}