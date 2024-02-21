using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfaceButtons : MonoBehaviour
{
    private PlayerInterfaceManager _manager;
    [SerializeField] private Button _handButton;
    [SerializeField] private Button _marketButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _chatButton;

    private void Start(){
        _manager = PlayerInterfaceManager.Instance;

        // Add event listeners to the buttons
        _handButton.onClick.AddListener(OnHandButtonPressed);
        _marketButton.onClick.AddListener(OnMarketButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _undoButton.onClick.AddListener(OnUndoButtonPressed);
        _chatButton.onClick.AddListener(OnChatButtonPressed);
    }
    public void DisableUtilityButton() => _utilityButton.interactable = false;
    public void OnHandButtonPressed() => _manager.OpenCardCollectionView();
    public void OnMarketButtonPressed() => _manager.OpenMarketView();
    public void OnUtilityButtonPressed() => _manager.ForceEndTurn();
    public void OnUndoButtonPressed() => _manager.Undo();
    public void OnChatButtonPressed() => _manager.OpenChat();
}

public enum UndoReason : byte
{
    PlayMoney,
    Attack,
    Block
}
