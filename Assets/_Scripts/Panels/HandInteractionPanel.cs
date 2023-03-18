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
    private int _nbCardsToTrashMax;

    // UI
    [SerializeField] private GameObject _interaction;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private GameObject _skipButton;

    [SerializeField] private TMP_Text _displayText;

    [SerializeField] private List<GameObject> selectedCardsList = new();
    public static event Action OnDiscardEnded;
    public static event Action OnTrashEnded;
    public static event Action OnDeployEnded;
    
    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPrepareHandInteractionPanel(int nbCardsToDiscard){
        _hand = Hand.Instance;
        _nbCardsToDiscard = nbCardsToDiscard;

        _interaction.SetActive(false);
        _skipButton.SetActive(false);
        _waitingText.SetActive(false);
    }

    #region Discard
    [ClientRpc]
    public void RpcBeginDiscard(){
        _state = TurnState.Discard;
        _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        _interaction.SetActive(true);
        _confirmButton.interactable = false;

        _hand.StartDiscard();
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
    #endregion

    #region Trash
    [TargetRpc]
    public void TargetBeginTrash(NetworkConnection conn, int nbCardsToTrash){
        _state = TurnState.Trash;
        _interaction.SetActive(true);

        if (nbCardsToTrash == 0){
            OnConfirmButtonPressed();
            return;
        }
        
        _displayText.text = $"Trash up to {nbCardsToTrash} cards";
        _confirmButton.interactable = true;
        _nbCardsToTrashMax = nbCardsToTrash;
        _hand.StartTrash();

    }

    public void CardTrashSelected(GameObject card, bool selected){
        if (selected) {
            selectedCardsList.Add(card);
        } else {
            selectedCardsList.Remove(card);
        }

        // if(selectedCardsList.Count >= _nbCardsToTrashMax) _hand.PreventMoreTrashing();
        // else _hand.AllowMoreTrashing();
    }
    
    [ClientRpc]
    public void RpcFinishTrashing(){
        ResetPanel();
        OnTrashEnded?.Invoke();
    }
    #endregion

    #region Deploy
    [TargetRpc]
    public void TargetBeginDeploy(NetworkConnection conn){
        _state = TurnState.Deploy;
        _displayText.text = $"You may deploy a card";
        selectedCardsList.Clear();

        _skipButton.SetActive(true);
        _waitingText.SetActive(false);
        _buttons.SetActive(true);

        _interaction.SetActive(true);
    }

    public void SelectCardToDeploy(GameObject card) => selectedCardsList.Add(card);
    public void DeselectCardToDeploy(GameObject card) => selectedCardsList.Remove(card);
    
    [ClientRpc]
    public void RpcEndDeploy(){
        ResetPanel();
        OnDeployEnded?.Invoke();
    }

    #endregion

    public void OnConfirmButtonPressed(){
        if(selectedCardsList.Count == 0) return;

        var player = PlayerManager.GetLocalPlayer();
        if (_state == TurnState.Deploy) player.CmdDeployCard(selectedCardsList[0]);
        else if (_state == TurnState.Discard) player.CmdDiscardSelection(selectedCardsList);
        else if (_state == TurnState.Trash) player.CmdTrashSelection(selectedCardsList);

        _buttons.SetActive(false);
        _waitingText.SetActive(true);
    }

    public void OnSkipButtonPressed(){
        _buttons.SetActive(false);
        _waitingText.SetActive(true);
        PlayerManager.GetLocalPlayer().CmdSkipDeploy();
    }

    private void ResetPanel(){
        _interaction.SetActive(false);
        _waitingText.SetActive(false);
        _skipButton.SetActive(false);
        _buttons.SetActive(true);
        selectedCardsList.Clear();
    }
}