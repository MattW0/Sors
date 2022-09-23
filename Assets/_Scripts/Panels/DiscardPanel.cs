using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPanel : NetworkBehaviour
{   
    public static DiscardPanel Instance { get; private set; }
    
    private int _nbSelected;
    private int _nbCardsToDiscard;
    public Button confirm;
    public TMP_Text displayText;
    public GameObject waitingText;

    [SerializeField] private List<GameObject> selectedCardsList;
    
    private void Awake() {
        if (!Instance) Instance = this;
        _nbCardsToDiscard = GameManager.Instance.nbDiscard;

        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
    }
    
    [ClientRpc]
    public void RpcBeginDiscard()
    {
        gameObject.SetActive(true);
        
        _nbSelected = 0;
        confirm.interactable = false;
        
        selectedCardsList = new List<GameObject>();
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        
        // OnDiscardPhaseStarted?.Invoke();
    }

    [ClientRpc]
    public void RpcFinishDiscard()
    {
        confirm.gameObject.SetActive(true);
        waitingText.SetActive(false);
        
        gameObject.SetActive(false);
        selectedCardsList.Clear();
    }

    [ClientRpc]
    public void RpcSetInactive()
    {
        gameObject.SetActive(false);
    }

    public void CardToDiscardSelected(GameObject card, bool selected){
        if (selected) {
            _nbSelected++;
            selectedCardsList.Add(card);
        } else {
            _nbSelected--;
            selectedCardsList.Remove(card);
        }

        displayText.text = $"Discard {_nbSelected}/{_nbCardsToDiscard} cards";

        if (_nbSelected == _nbCardsToDiscard) confirm.interactable = true;
        else confirm.interactable = false;
    }

    public void ConfirmButtonPressed(){
        confirm.gameObject.SetActive(false);
        waitingText.SetActive(true);

        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDiscardSelection(selectedCardsList);
    }
}