using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerInterfaceButtons : MonoBehaviour
{
    private PlayerInterfaceManager _manager;
    [SerializeField] private Button _marketButton;
    [SerializeField] private Button _prevailButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _chatButton;
    public static event Action OnOpenMarket;
    public static event Action OnOpenPrevailPanel;
    public static event Action OnQuitButtonClicked;

    private void Start(){
        _manager = PlayerInterfaceManager.Instance;

        // Add event listeners to the buttons
        _marketButton.onClick.AddListener(OnMarketButtonClicked);
        _prevailButton.onClick.AddListener(OnPrevailButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _chatButton.onClick.AddListener(OnChatButtonPressed);
    }
    public void OnMarketButtonClicked() => OnOpenMarket?.Invoke();
    public void OnPrevailButtonPressed() => OnOpenPrevailPanel?.Invoke();
    // public void OnUtilityButtonPressed() => _manager.ForceEndTurn();
    public void OnUtilityButtonPressed() => OnQuitButtonClicked?.Invoke();
    public void OnChatButtonPressed() => _manager.ToggleChat();
}
