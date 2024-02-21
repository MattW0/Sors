using System;
using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInterfaceManager : NetworkBehaviour
{
    public static PlayerInterfaceManager Instance { get; private set; }
    [SerializeField] private Logger _logger;
    [SerializeField] private Chat _chat;
    public static event Action<string> OnChatMessageSent;

    private PlayerManager _player;
    private Market _market;
    private HandInteractionPanel _cardCollectionPanel;
    
    private void Awake()
    {
        if (!Instance) Instance = this;
        
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
    public void RpcLog(string message, LogType type) => _logger.Log(message, type);
    
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