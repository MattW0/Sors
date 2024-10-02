using System;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour 
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Transform _chatTransform;
    [SerializeField] private TextMessage _textMessagePrefab;
    // private string _playerName;
    private PlayerInterfaceManager _manager;

    private void Awake()
    {
        _inputField.onEndEdit.AddListener(InputFieldEdit);
    }

    private void Start() 
    {
        _manager = PlayerInterfaceManager.Instance;
        PlayerInterfaceManager.OnChatMessageReceived += HandleNewMessage;
    }

    private void InputFieldEdit(string message) 
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        print("message on client: " + message);

        _manager.Send(message);
        _inputField.text = string.Empty;

        // chatLog.text += "\n" + message;
        // print("message: " + message);
    }

    private void HandleNewMessage(string message) 
    {
        var msg = Instantiate(_textMessagePrefab, _chatTransform);
        msg.SetMessage(message);
    }

    public void ToggleChat() {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OnDestroy() 
    {
        PlayerInterfaceManager.OnChatMessageReceived -= HandleNewMessage;
    }
}
