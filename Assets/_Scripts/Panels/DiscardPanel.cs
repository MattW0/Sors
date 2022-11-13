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
    
    private int _nbCardsToDiscard;
    public Button confirm;
    public TMP_Text displayText;
    public GameObject waitingText;

    [SerializeField] private List<GameObject> selectedCardsList;
    public static event Action OnDiscardPhaseEnded;
    
    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPrepareDiscardPanel(int nbCardsToDiscard)
    {
        _nbCardsToDiscard = nbCardsToDiscard;
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";

        confirm.interactable = false;
        waitingText.SetActive(false);

        gameObject.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcBeginDiscard()
    {
        gameObject.SetActive(true);
        selectedCardsList = new List<GameObject>();
    }

    [ClientRpc]
    public void RpcFinishDiscard()
    {
        confirm.gameObject.SetActive(true);
        waitingText.SetActive(false);
        
        gameObject.SetActive(false);
        
        OnDiscardPhaseEnded?.Invoke();
    }

    [ClientRpc]
    public void RpcSetInactive(){
        gameObject.SetActive(false);
    }

    public void CardToDiscardSelected(GameObject card, bool selected){
        
        if (selected) {
            selectedCardsList.Add(card);
        } else {
            selectedCardsList.Remove(card);
        }

        var nbSelected = selectedCardsList.Count;
        displayText.text = $"Discard {nbSelected}/{_nbCardsToDiscard} cards";
        confirm.interactable = nbSelected == _nbCardsToDiscard;
    }

    public void ConfirmButtonPressed(){
        confirm.gameObject.SetActive(false);
        waitingText.SetActive(true);

        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDiscardSelection(selectedCardsList);
    }
}