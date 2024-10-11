using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class AlertDialogue : ModalWindow
{
    [Header("Resources")]
    [SerializeField] private TMP_Text _windowTitle;
    [SerializeField] private TMP_Text _windowDescription;
    [SerializeField] private AnimatedButton _acceptButton;
    [SerializeField] private AnimatedButton _declineButton;
    [SerializeField] private ModalWindowType _windowType;

    [Header("Settings")]
    public string titleText = "Title";
    [TextArea] public string descriptionText = "Description here";
    public string acceptButtonText = "Ok";
    public string declineButtonText = "Cancel";

    public static event Action<ModalWindowType> OnAccept;
    public static event Action<ModalWindowType> OnDecline;

    private void Start()
    {
        _windowTitle.text = titleText;
        _windowDescription.text = descriptionText;

        _acceptButton.buttonText = acceptButtonText;
        _declineButton.buttonText = declineButtonText;

        _acceptButton.gameObject.GetComponent<Button>().onClick.AddListener(Accept);
        _declineButton.gameObject.GetComponent<Button>().onClick.AddListener(Decline);

        gameObject.SetActive(false);
    }

    public void Accept()
    {
        OnAccept?.Invoke(_windowType);
        ModalWindowOut();
    }

    private void Decline()
    {
        OnDecline?.Invoke(_windowType);
        ModalWindowOut();
    }

    public void SetMessage(string message)
    {
        _windowDescription.text = message;
    }
}

public enum ModalWindowType
{
    INFO = 0,
    WARNING = 1,
    ERROR = 2,
    EXIT = 10,
    LOBBY_INVITE = 11,
}