using System;
using UnityEngine;
using TMPro;
using Mirror;

public class Chat : NetworkBehaviour {
    
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text chatLog;

    private static event Action<string> onMessageSent;

    public override void OnStartServer() {
        gameObject.SetActive(false); 
        return;

        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
        networkIdentity.AssignClientAuthority(connectionToClient);

        onMessageSent += HandleNewMessage;
    }

    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        onMessageSent -= HandleNewMessage;
    }

    private void HandleNewMessage(string message) {
        chatLog.text += "\n" + message;
        print("message: " + message);
    }

    [Client]
    public void Send(string message) {
        // if (!Input.GetKeyDown(KeyCode.Return)) return;
        // if (string.IsNullOrWhiteSpace(message)) return;

        CmdSendMessage(message);
        
        inputField.text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message) {
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message) {
        onMessageSent?.Invoke(message);
    }
}
