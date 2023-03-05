using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandInteractionPanel : NetworkBehaviour
{   
    public static HandInteractionPanel Instance { get; private set; }
    private Hand _hand;
    private int _nbCardsToDiscard;
    private int _nbCardsToTrash;
    private TurnState _state;
    public Button confirm;
    public TMP_Text displayText;
    public GameObject waitingText;

    [SerializeField] private List<GameObject> selectedCardsList = new();
    public static event Action OnDiscardEnded;
    public static event Action OnTrashEnded;
    
    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPrepareDiscardPanel(int nbCardsToDiscard){
        _hand = Hand.Instance;
        _nbCardsToDiscard = nbCardsToDiscard;

        confirm.interactable = false;
        waitingText.SetActive(false);
        gameObject.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcBeginDiscard(){
        _state = TurnState.Discard;
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        gameObject.SetActive(true);
        confirm.interactable = false;

        _hand.StartDiscard(true);
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

    [ClientRpc]
    public void RpcFinishDiscard(){
        ResetPanel();
        OnDiscardEnded?.Invoke();
    }

    [TargetRpc]
    public void TargetBeginTrash(NetworkConnection conn, int nbCardsToTrash){
        _state = TurnState.Trash;
        _nbCardsToTrash = nbCardsToTrash;
        displayText.text = $"Trash up to {nbCardsToTrash} cards";
        gameObject.SetActive(true);
        confirm.interactable = true;

        if (nbCardsToTrash == 0) ConfirmButtonPressed();
        else _hand.StartTrash(true);
    }

    public void CardTrashSelected(GameObject card, bool selected){
        if (selected) {
            selectedCardsList.Add(card);
        } else {
            selectedCardsList.Remove(card);
        }
    }

    [ClientRpc]
    public void RpcFinishTrashing(){
        ResetPanel();
        OnTrashEnded?.Invoke();
    }

    public void ConfirmButtonPressed(){
        confirm.gameObject.SetActive(false);
        waitingText.SetActive(true);

        var player = PlayerManager.GetLocalPlayer();
        if (_state == TurnState.Discard) player.CmdDiscardSelection(selectedCardsList);
        if (_state == TurnState.Trash) player.CmdTrashSelection(selectedCardsList);
    }

    private void ResetPanel(){
        confirm.gameObject.SetActive(true);
        waitingText.SetActive(false);
        gameObject.SetActive(false);
        selectedCardsList.Clear();
    }
}