using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardCollectionPanelUI : MonoBehaviour
{   
    [Header("Entities")]
    [SerializeField] private CardCollectionPanel _cardCollectionPanel;
    // private Hand _hand;
    

    [Header("UI")]
    [SerializeField] private GameObject _interaction;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private GameObject _skipButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private TMP_Text _selectionTitle;
    [SerializeField] private TMP_Text _displayText;

    [Header("Helper Fields")]
    private TurnState _state;
    [SerializeField] private List<GameObject> selectedCardsList = new();
    private int _nbCardsToDiscard;
    private int _nbCardsToTrashMax;
    public static event Action OnDiscardEnded;
    public static event Action OnTrashEnded;
    public static event Action OnDeployEnded;
    
    private void Awake() {
        // if (!Instance) Instance = this;

        // _cardCollectionView = CardCollectionView.Instance;
    }

    public void PrepareCardCollectionPanelUi(int nbCardsToDiscard){
        // _hand = Hand.Instance;
        _nbCardsToDiscard = nbCardsToDiscard;

        _interaction.SetActive(false);
        _buttons.SetActive(false);
        _skipButton.SetActive(false);

        _waitingText.SetActive(false);
        _displayText.text = "";
    }

    #region Discard
    public void BeginDiscard(){
        _state = TurnState.Discard;

        _collectionTitle.text = "Hand";
        _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";

        _buttons.SetActive(true);
        _interaction.SetActive(true);
        _confirmButton.interactable = false;

        // _hand.StartDiscard();
    }

    public void UpdateDiscardPanel(int nbSelected){
        _displayText.text = $"Discard {nbSelected}/{_nbCardsToDiscard} cards";
        _confirmButton.interactable = nbSelected == _nbCardsToDiscard;
    }

    public void FinishDiscard(){
        ResetPanelUI();
        OnDiscardEnded?.Invoke();
    }
    #endregion

    #region Deploy
    public void BeginDeploy(){
        _state = TurnState.Deploy;
        selectedCardsList.Clear();

        _displayText.text = $"You may deploy a card";
        _skipButton.SetActive(true);
        _waitingText.SetActive(false);
        _buttons.SetActive(true);

        _interaction.SetActive(true);
    }

    public void SelectCardToDeploy(GameObject card) => selectedCardsList.Add(card);
    public void DeselectCardToDeploy(GameObject card) => selectedCardsList.Remove(card);
    
    public void FinishDeploy(){
        ResetPanelUI();
        OnDeployEnded?.Invoke();
    }

    #endregion

    #region Trash
    public void BeginTrash(int nbCardsToTrash){
        _state = TurnState.Trash;
        _interaction.SetActive(true);
        _buttons.SetActive(true);

        if (nbCardsToTrash == 0){
            SkipTrash();
            return;
        }
        
        _displayText.text = $"Trash up to {nbCardsToTrash} cards";
        _confirmButton.interactable = true;
        _nbCardsToTrashMax = nbCardsToTrash;
        // _hand.StartTrash();

    }

    public void CardTrashSelected(GameObject card, bool selected){
        if (selected) {
            selectedCardsList.Add(card);
        } else {
            selectedCardsList.Remove(card);
        }
    }
    
    public void FinishTrash(){
        ResetPanelUI();
        OnTrashEnded?.Invoke();
    }
    #endregion

    public void OnCloseButtonPressed(){
        ResetPanelUI();
    }

    public void OnConfirmButtonPressed(){

        // if (_state == TurnState.Deploy) player.CmdDeployCard(selectedCardsList[0]);
        // else if (_state == TurnState.Trash) player.CmdTrashSelection(selectedCardsList);

        if (_state == TurnState.Discard) _cardCollectionPanel.ConfirmDiscard();

        _buttons.SetActive(false);
        _waitingText.SetActive(true);
    }

    public void SkipTrash(){
        _buttons.SetActive(false);
        _waitingText.SetActive(true);
        PlayerManager.GetLocalPlayer().CmdSkipTrash();
    }

    public void OnSkipButtonPressed(){
        _buttons.SetActive(false);
        _waitingText.SetActive(true);
        PlayerManager.GetLocalPlayer().CmdSkipDeploy();
    }

    private void ResetPanelUI(){
        _interaction.SetActive(false);
        _waitingText.SetActive(false);
        _skipButton.SetActive(false);
        _buttons.SetActive(true);
        selectedCardsList.Clear();
    }
}