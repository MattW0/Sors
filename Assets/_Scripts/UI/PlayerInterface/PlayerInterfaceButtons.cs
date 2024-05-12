using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfaceButtons : MonoBehaviour
{
    private PlayerInterfaceManager _manager;
    [SerializeField] private Button _marketButton;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _concedeButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _chatButton;

    private void Start(){
        _manager = PlayerInterfaceManager.Instance;

        // Add event listeners to the buttons
        _marketButton.onClick.AddListener(OnMarketButtonPressed);
        _undoButton.onClick.AddListener(OnUndoButtonPressed);
        _concedeButton.onClick.AddListener(OnConcedeButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _chatButton.onClick.AddListener(OnChatButtonPressed);
    }
    public void OnMarketButtonPressed() => _manager.OpenMarketView();
    public void OnUndoButtonPressed() => _manager.Undo();
    public void OnConcedeButtonPressed() => _manager.Concede();
    public void OnUtilityButtonPressed() => _manager.ForceEndTurn();
    public void DisableUtilityButton() => _utilityButton.interactable = false;
    public void UndoButtonEnabled(bool b) => _undoButton.interactable = b;
    public void OnChatButtonPressed() => _manager.OpenChat();
}

public enum UndoReason : byte
{
    PlayMoney,
    Attack,
    Block
}
