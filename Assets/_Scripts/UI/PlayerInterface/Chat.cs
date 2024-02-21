using System;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour 
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text chatLog;

    private PlayerInterfaceManager _manager;

    private void Start() {
        gameObject.SetActive(false);

        _manager = PlayerInterfaceManager.Instance;
        PlayerInterfaceManager.OnChatMessageSent += HandleNewMessage;
    }

    private void Update()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) return;
        if (!Input.GetKeyDown(KeyCode.Return)) return;

        _manager.Send(inputField.text);
        inputField.text = string.Empty;
    }

    private void HandleNewMessage(string message) {
        chatLog.text += "\n" + message;
        print("message: " + message);
    }

    public void ToggleChat() {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OnDestroy() {
        PlayerInterfaceManager.OnChatMessageSent -= HandleNewMessage;
    }
}
