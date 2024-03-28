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
    private PlayerInterfaceButtons _buttons;
    public static event Action<string> OnChatMessageSent;

    private PlayerManager _player;
    private Market _market;
    private HandInteractionPanel _cardCollectionPanel;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _buttons = GetComponent<PlayerInterfaceButtons>();
        GameManager.OnGameStart += RpcPrepareUIs;
    }

    [ClientRpc]
    private void RpcPrepareUIs(GameOptions gameOptions){

        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
        if(connectionToClient != null) networkIdentity.AssignClientAuthority(connectionToClient);

        _market = Market.Instance;
        _cardCollectionPanel = HandInteractionPanel.Instance;

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) gameObject.GetComponent<PlayerInterfaceButtons>().DisableUtilityButton();
    }

    [ClientRpc]
    public void RpcUndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    [TargetRpc]
    public void TargetUndoButtonEnabled(NetworkConnection conn, bool b) => _buttons.UndoButtonEnabled(b);
    public void UndoButtonEnabled(bool b) => _buttons.UndoButtonEnabled(b);

    [ClientRpc]
    public void RpcLog(string message, LogType type) => _logger.Log(message, type);
    [ClientRpc]
    public void RpcLogGameStart(List<string> playerNames)
    {
        var msg = "--- Game Setup ---\n";

        if (playerNames.Count == 1)  msg += $"{playerNames[0]} vs Computer\n";
        else msg += $"{playerNames[0]} vs {playerNames[1]}\n";

        _logger.Log(msg, LogType.Standard);
    }

    [ClientRpc]
    public void RpcPlayCards(List<BattleZoneEntity> entities){
        foreach (var e in entities) _logger.Log($"{e.Owner.PlayerName} plays {e.Title}", LogType.Play);
    }
    
    [Client]
    public void Send(string message) {
        CmdSendMessage(message);
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message) {
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message) {
        OnChatMessageSent?.Invoke(message);
    }
    
    public void OpenCardCollectionView() => _cardCollectionPanel.ToggleView();
    public void OpenMarketView() => _market.MaxButton();
    public void ForceEndTurn() => _player.ForceEndTurn();
    public void Undo() => _player.CmdUndoPlayMoney();
    public void OpenChat() => _chat.ToggleChat();

    private void OnDestroy(){
        GameManager.OnGameStart -= RpcPrepareUIs;
    }
}