using System;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerInterfaceButtons))]
public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    [SerializeField] private Chat _chat;
    [SerializeField] private ActionDescription _actionDescription;
    private PlayerInterfaceButtons _buttons;
    private PlayerManager _player;
    private Dictionary<int, string> _messageOrigin = new();
    public static event Action<string, string> OnChatMessageReceived;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _buttons = GetComponent<PlayerInterfaceButtons>();
        TurnManager.OnBeginTurn += RpcBeginTurn;
        TurnManager.OnTurnStateChanged += RpcChangeActionDescriptionText;
    }

    [ClientRpc]
    public void RpcPrepare(PlayerManager[] players, int numberPhasesToChoose)
    {
        _actionDescription.NumberPhases = numberPhasesToChoose;

        _messageOrigin.Add(0, "Game"); // Engine actions like phase changes etc.
        foreach (var p in players) {
            if(p.ID == 1) _messageOrigin.Add(p.ID, p.PlayerName.AddColor(SorsColors.player));
            else _messageOrigin.Add(p.ID, p.PlayerName.AddColor(SorsColors.opponent));
        }

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) gameObject.GetComponent<PlayerInterfaceButtons>().DisableUtilityButton();
        // else TurnManager.OnPlayerIsReady += RpcLogPlayerAction;

        PrintGameStart();
    }

    [ClientRpc]
    private void RpcChangeActionDescriptionText(TurnState state) => _actionDescription.ChangeActionDescriptionText(state);

    [ClientRpc]
    private void RpcBeginTurn(int turnNumber) => _actionDescription.StartTurn(turnNumber);
    // [ClientRpc]
    // public void RpcUndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    // [TargetRpc]
    // public void TargetUndoButtonEnabled(NetworkConnection conn, bool b) => _buttons.UndoButtonEnabled(b);
    // public void UndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    #region Log

    [ClientRpc] public void RpcTurnStart(int turnNumber) => _logger.Log($"Turn {turnNumber}", _messageOrigin[0], LogType.TurnChange);
    [ClientRpc] public void RpcEndGame(int winnerId) => _logger.Log("Wins the game!", _messageOrigin[winnerId], LogType.Standard);
    [ClientRpc] public void RpcPhasesToPlay(List<TurnState> phases)
    {
        var msg = $"Phases to play:";
        msg += string.Join(", ", phases.Select(phase => phase.ToString()));

        _logger.Log(msg, _messageOrigin[0], LogType.Standard);
    }
    [ClientRpc] public void RpcPhaseChange(TurnState phase)
    {
        var text = phase.ToString();
        if (phase == TurnState.Attackers) text = "Combat";

        _logger.Log(text, _messageOrigin[0], LogType.Phase);
    }
    [ClientRpc] public void RpcCardDraw(int playerId, int number) => _logger.Log($"Draws {number} cards", _messageOrigin[playerId], LogType.Standard);
    [ClientRpc] public void RpcDiscardCards(int playerId, string cards) => _logger.Log($"Discards {cards}", _messageOrigin[playerId], LogType.Standard);
    [ClientRpc] internal void RpcBuyCard(int playerId, string cardName, int cost) => _logger.Log($"Buys {cardName} for {cost} cash", _messageOrigin[playerId], LogType.Buy);
    [ClientRpc] public void RpcPlayCard(int playerId, string cardName, int cost) => _logger.Log($"Plays {cardName} for {cost} cash", _messageOrigin[playerId], LogType.Play);
    [ClientRpc] public void RpcPlayerTargeting(int playerId, string sourceTitle, string targetTitle, LogType type)
    {
        if (type == LogType.CombatAttacker) _logger.Log($"{sourceTitle} attacks {targetTitle}", _messageOrigin[playerId], LogType.CombatAttacker);
        else if (type == LogType.CombatBlocker) _logger.Log($"{sourceTitle} blocks {targetTitle}", _messageOrigin[playerId], LogType.CombatBlocker);
        else if (type == LogType.AbilityTarget) _logger.Log($"{sourceTitle} targets {targetTitle}", _messageOrigin[playerId], LogType.CombatClash);
    }

    [ClientRpc] internal void RpcAbility(int iD, string source, string target, LogType type)
    {
        if (type == LogType.AbilityExecution) _logger.Log($"Executing ability from {source} with target {target}", _messageOrigin[iD], type);
    }

    [ClientRpc] public void RpcCombatClash(int playerId, string clash) => _logger.Log(clash, _messageOrigin[playerId], LogType.CombatClash);

    private void PrintGameStart()
    {
        var msg = "";
        if (_messageOrigin.Count == 2)  msg += $" --- {_messageOrigin[1]} vs Computer --- ";
        else msg += $" --- {_messageOrigin[1]} vs {_messageOrigin[2]} --- ";
        _logger.Log(msg, _messageOrigin[0], LogType.Standard);
    }

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

    // Only used for undo on playing money cards
    // TODO: Redo attackers, blockers choices
    public void Undo() => _player.CmdUndoPlayMoney();
    public void ForceEndTurn() => _player.ForceEndTurn();

    private void OnDestroy()
    {
        // TurnManager.OnPlayerIsReady -= RpcLogPlayerAction;
        TurnManager.OnTurnStateChanged -= RpcChangeActionDescriptionText;
    }
}