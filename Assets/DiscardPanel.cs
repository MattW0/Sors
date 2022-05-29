using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPanel : NetworkBehaviour
{   
    public static DiscardPanel instance;
    private TurnManager turnManager;
    private int _nbSelected;
    public Button confirm;
    public TMP_Text displayText;

    private List<CardInfo> _selectedCardsList;
    
    private void Awake() {
        
        if (instance == null) instance = this;

        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        turnManager = TurnManager.instance;
    }

    public void CardToDiscardSelected(bool selected){
        
        if (selected) _nbSelected++;
        else _nbSelected--;

        displayText.text = $"{_nbSelected} cards selected";

        if (_nbSelected == 2) confirm.interactable = true;
        else confirm.interactable = false;
    }

    public void ConfirmButtonPressed(){
        confirm.interactable = false;

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDiscardSelection(_selectedCardsList);
    }
}