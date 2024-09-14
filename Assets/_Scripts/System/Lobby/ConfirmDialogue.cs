using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialogue : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _declineButton;
    public static event Action OnAccept;
    public static event Action OnDecline;

    private void Start()
    {
        _acceptButton.onClick.AddListener(Accept);
        _declineButton.onClick.AddListener(Decline);
    }

    public void Accept()
    {
        gameObject.SetActive(false);
        OnAccept?.Invoke();
    }

    public void Decline()
    {
        gameObject.SetActive(false);
        OnDecline?.Invoke();
    }

    internal void SetMessage(string message)
    {
        _messageText.text = message;
    }
}
