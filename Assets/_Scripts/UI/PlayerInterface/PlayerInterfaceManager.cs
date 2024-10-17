using System;
using Mirror;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInterfaceButtons))]
public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    [SerializeField] private Chat _chat;
    [SerializeField] private ActionDescription _actionDescription;
    private PlayerInterfaceButtons _buttons;
    private PlayerManager _player;
    public static event Action<string> OnChatMessageReceived;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _buttons = GetComponent<PlayerInterfaceButtons>();
        GameManager.OnGameStart += RpcPrepareUIs;
        TurnManager.OnPhaseChanged += RpcChangeActionDescriptionText;
    }

    [ClientRpc]
    private void RpcPrepareUIs(GameOptions gameOptions)
    {
        _actionDescription.NumberPhases = gameOptions.NumberPhases;

        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
        if(connectionToClient != null) networkIdentity.AssignClientAuthority(connectionToClient);

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) gameObject.GetComponent<PlayerInterfaceButtons>().DisableUtilityButton();
    }

    [ClientRpc]
    private void RpcChangeActionDescriptionText(TurnState state) => _actionDescription.ChangeActionDescriptionText(state);

    // [ClientRpc]
    // public void RpcUndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    // [TargetRpc]
    // public void TargetUndoButtonEnabled(NetworkConnection conn, bool b) => _buttons.UndoButtonEnabled(b);
    // public void UndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    [ClientRpc]
    public void RpcLog(string message, LogType type) => _logger.Log(message, type);
    [ClientRpc]
    public void RpcLogGameStart(List<string> playerNames)
    {
        var msg = "";
        if (playerNames.Count == 1)  msg += $" --- {playerNames[0]} vs Computer --- ";
        else msg += $" --- {playerNames[0]} vs {playerNames[1]} --- ";

        _logger.Log(msg, LogType.Standard);
    }

    [ClientRpc]
    public void RpcLogPlayingCards(List<BattleZoneEntity> entities){
        foreach (var e in entities) _logger.Log($"{e.Owner.PlayerName} plays {e.Title}", LogType.Play);
    }

    [Client]
    public void Send(string message) => CmdSendMessage(message);

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message) => RpcHandleMessage($"{message}");

    [ClientRpc]
    private void RpcHandleMessage(string message) => OnChatMessageReceived?.Invoke(message);
    // Only used for undo on playing money cards
    // TODO: Redo attackers, blockers choices
    public void Undo() => _player.CmdUndoPlayMoney();
    public void Concede() {} // TODO
    public void ForceEndTurn() => _player.ForceEndTurn();
    public void ToggleLogChat()
    {
        _chat.ToggleVisible();
        _logger.ToggleVisible();
    }

    private void OnDestroy(){
        GameManager.OnGameStart -= RpcPrepareUIs;
    }
}