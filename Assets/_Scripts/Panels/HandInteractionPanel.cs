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
    private TurnState _state;
    private Hand _hand;
    private int _nbCardsToDiscard;
    private int _nbCardsToTrash;

    // UI
    [SerializeField] private GameObject _interaction;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _displayText;

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

        _confirmButton.interactable = false;
        _interaction.SetActive(false);
        _waitingText.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcBeginDiscard(){
        _state = TurnState.Discard;
        _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        _interaction.SetActive(true);
        _confirmButton.interactable = false;

        _hand.StartDiscard(true);
    }

    public void CardToDiscardSelected(GameObject card, bool selected){  
        if (selected) {
            selectedCardsList.Add(card);
        } else {
            selectedCardsList.Remove(card);
        }

        var nbSelected = selectedCardsList.Count;
        _displayText.text = $"Discard {nbSelected}/{_nbCardsToDiscard} cards";
        _confirmButton.interactable = nbSelected == _nbCardsToDiscard;
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
        _displayText.text = $"Trash up to {nbCardsToTrash} cards";
        _interaction.SetActive(true);
        _confirmButton.interactable = true;

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

    private void ConfirmButtonPressed(){
        _confirmButton.gameObject.SetActive(false);
        _waitingText.SetActive(true);

        var player = PlayerManager.GetLocalPlayer();
        if (_state == TurnState.Discard) player.CmdDiscardSelection(selectedCardsList);
        if (_state == TurnState.Trash) player.CmdTrashSelection(selectedCardsList);
    }

    private void ResetPanel(){
        _confirmButton.gameObject.SetActive(true);
        _interaction.SetActive(false);
        _waitingText.SetActive(false);
        selectedCardsList.Clear();
    }
}