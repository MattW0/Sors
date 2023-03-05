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
    [SerializeField] private Button _readyButton;


    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start(){
        _kingdom = Kingdom.Instance;
        _player = PlayerManager.GetLocalPlayer();
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
