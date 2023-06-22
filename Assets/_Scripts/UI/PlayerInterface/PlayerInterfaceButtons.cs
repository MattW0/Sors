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
    private bool _isOpen = false;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _kingdom = Kingdom.Instance;
        _cardCollectionPanel = CardCollectionPanel.Instance;

        _player = PlayerManager.GetLocalPlayer();
        if(!_player.isServer) _utilityButton.interactable = false;
    }

    // public void OnHandButtonPressed(){
    //     if(_isOpen) {
    //         _isOpen = false;
    //         _cardCollectionPanel.ClearPanel();
    //         return;
    //     }

    //     _isOpen = true;
    //     _player.PlayerClickedCollectionViewButton();
    // }

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
