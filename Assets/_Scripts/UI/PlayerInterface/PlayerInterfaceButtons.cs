using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInterfaceButtons : MonoBehaviour
{
    public static PlayerInterfaceButtons Instance { get; private set; }
    private PlayerManager _player;
    private Kingdom _kingdom;
    private CardCollectionPanel _cardCollectionPanel;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _handButton;
    [SerializeField] private Button _kingdomButton;

    // private bool _isOpen = false;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _kingdom = Kingdom.Instance;
        _cardCollectionPanel = CardCollectionPanel.Instance;

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) _utilityButton.interactable = false;

        // Add event listeners to the buttons
        _handButton.onClick.AddListener(OnHandButtonPressed);
        _kingdomButton.onClick.AddListener(OnKingdomButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _undoButton.onClick.AddListener(OnUndoButtonPressed);
    }

    public void OnHandButtonPressed(){
        _cardCollectionPanel.ToggleView();
    }

    public void OnKingdomButtonPressed(){
        _kingdom.MaxButton();
    }
    
    public void OnUtilityButtonPressed(){
        _player.ForceEndTurn();
    }
    
    public void OnUndoButtonPressed() {
        _player.CmdUndoPlayMoney();
    }
}

public enum UndoReason
{
    PlayMoney,
    Attack,
    Block
}
