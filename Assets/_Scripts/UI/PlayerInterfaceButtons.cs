using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInterfaceButtons : MonoBehaviour
{
    public static PlayerInterfaceButtons Instance { get; private set; }
    private Kingdom _kingdom;
    [SerializeField] private Button _readyButton;


    private void Awake()
    {
        if (!Instance) Instance = this;        
    }

    private void Start(){
        _kingdom = Kingdom.Instance;
    }

    public void OnKingdomButtonPressed(){
        _kingdom.MaxButton();
    }
    
    public void OnResignButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
    }
    
    public void OnUndoButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
    }
    
    public void OnReadyButtonPressed() {
        var player = PlayerManager.GetPlayerManager();
        player.PlayerPressedReadyButton();
        
        _readyButton.interactable = false;
    }

    public void EnableReadyButton(){
        _readyButton.interactable = true;
    }

    public void DisableReadyButton(){
        _readyButton.interactable = false;
    }

}
