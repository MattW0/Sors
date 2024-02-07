using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInterfaceButtons : MonoBehaviour
{
    public static PlayerInterfaceButtons Instance { get; private set; }
    private PlayerManager _player;
    private Market _market;
    private HandInteractionPanel _cardCollectionPanel;
    [SerializeField] private Button _undoButton;
    [SerializeField] private Button _utilityButton;
    [SerializeField] private Button _handButton;
    [SerializeField] private Button _marketButton;

    // private bool _isOpen = false;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _market = Market.Instance;
        _cardCollectionPanel = HandInteractionPanel.Instance;

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) _utilityButton.interactable = false;

        // Add event listeners to the buttons
        _handButton.onClick.AddListener(OnHandButtonPressed);
        _marketButton.onClick.AddListener(OnMarketButtonPressed);
        _utilityButton.onClick.AddListener(OnUtilityButtonPressed);
        _undoButton.onClick.AddListener(OnUndoButtonPressed);
    }

    public void OnHandButtonPressed(){
        _cardCollectionPanel.ToggleView();
    }

    public void OnMarketButtonPressed(){
        _market.MaxButton();
    }
    
    public void OnUtilityButtonPressed(){
        _player.ForceEndTurn();
    }
    
    public void OnUndoButtonPressed() {
        _player.CmdUndoPlayMoney();
    }
}

public enum UndoReason : byte
{
    PlayMoney,
    Attack,
    Block
}
