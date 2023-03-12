using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInterfaceButtons : MonoBehaviour
{
    private PlayerManager _player;
    public static PlayerInterfaceButtons Instance { get; private set; }
    private Kingdom _kingdom;
    private CardCollectionView _cardCollectionView;
    [SerializeField] private Button _readyButton;
    private bool _isOpen = false;



    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _kingdom = Kingdom.Instance;
        _cardCollectionView = CardCollectionView.Instance;
        _player = PlayerManager.GetLocalPlayer();
    }

    public void OnHandButtonPressed(){
        if(_isOpen) {
            _isOpen = false;
            _cardCollectionView.CloseView();
            return;
        }

        _isOpen = true;
        _player.PlayerClickedCollectionViewButton();
    }

    public void OnKingdomButtonPressed(){
        _kingdom.MaxButton();
    }
    
    public void OnResignButtonPressed() {
    }
    
    public void OnUndoButtonPressed() {
    }
    
    public void OnReadyButtonPressed() {
        _player.PlayerPressedReadyButton();
        _readyButton.interactable = false;
    }

    public void EnableReadyButton(){
        _readyButton.interactable = true;
    }

    public void DisableReadyButton(){
        _readyButton.interactable = false;
    }

}
