using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInterfaceButtons : MonoBehaviour
{
    private PlayerInterfaceManager _manager;
    [SerializeField] private Button _marketButton;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _concedeButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _chatButton;
    [SerializeField] private TMP_Text _chatButtonText;
    private bool _chatVisible = false;
    public static event Action OnOpenMarket;
    public static event Action OnQuitButtonClicked;

    private void Start(){
        _manager = PlayerInterfaceManager.Instance;

        // Add event listeners to the buttons
        _marketButton.onClick.AddListener(OnMarketButtonClicked);
        _undoButton.onClick.AddListener(OnUndoButtonPressed);
        _concedeButton.onClick.AddListener(OnConcedeButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _chatButton.onClick.AddListener(OnChatButtonPressed);
    }
    public void OnMarketButtonClicked() => OnOpenMarket?.Invoke();
    public void OnConcedeButtonPressed() => OnQuitButtonClicked?.Invoke();
    public void OnUndoButtonPressed() => _manager.Undo();
    public void OnUtilityButtonPressed() => _manager.ForceEndTurn();
    public void DisableUtilityButton() => _utilityButton.interactable = false;
    public void UndoButtonEnabled(bool b) => _undoButton.interactable = b;
    public void OnChatButtonPressed()
    {
        _chatVisible = !_chatVisible;
        _chatButtonText.text = _chatVisible ? "Log" : "Chat";

        _manager.ToggleLogChat();
    }
}

public enum UndoReason : byte
{
    PlayMoney,
    Attack,
    Block
}
