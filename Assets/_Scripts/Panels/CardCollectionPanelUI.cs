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
    [SerializeField] private GameObject _container;
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
    private int _nbCardsToDiscard;
    private int _nbCardsToDeploy = 1;
    private int _nbCardsToTrashMax;
    public static event Action OnTrashEnded;
    public static event Action OnDeployEnded;
    
    private void Awake() {
        // if (!Instance) Instance = this;

        // _cardCollectionView = CardCollectionView.Instance;
    }

    public void PrepareCardCollectionPanelUi(int nbCardsToDiscard){
        _nbCardsToDiscard = nbCardsToDiscard;

        _interaction.SetActive(false);
        _buttons.SetActive(false);
        _skipButton.SetActive(false);

        _waitingText.SetActive(false);
        _displayText.text = "";
    }
    public void BeginDiscard(){
        _state = TurnState.Discard;
        InteractionBegin();
    }
    public void BeginDeploy(){
        _state = TurnState.Deploy;
        InteractionBegin();
    }

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
    }

    public void CardTrashSelected(GameObject card, bool selected){
        // if (selected) {
        //     selectedCardsList.Add(card);
        // } else {
        //     selectedCardsList.Remove(card);
        // }
    }
    #endregion

    public void OnConfirmButtonPressed(){

        // 
        // else if (_state == TurnState.Trash) player.CmdTrashSelection(selectedCardsList);

        if (_state == TurnState.Discard) _cardCollectionPanel.ConfirmDiscard();
        else if (_state == TurnState.Deploy) _cardCollectionPanel.ConfirmDeploy();

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

    private void InteractionBegin(){
        _buttons.SetActive(true);
        _interaction.SetActive(true);
        _waitingText.SetActive(false);
        _confirmButton.interactable = false;
        _collectionTitle.text = "Hand";

        switch (_state){
            case TurnState.Discard:
                _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";                
                break;
            case TurnState.Deploy:
                _skipButton.SetActive(true);
                _displayText.text = $"You may deploy a card";
                break;
        }
    }

    public void UpdateInteractionElements(int nbSelected){
        switch (_state){
            case TurnState.Discard:
                _displayText.text = $"Discard {nbSelected}/{_nbCardsToDiscard} cards";
                _confirmButton.interactable = nbSelected == _nbCardsToDiscard;
                break;
            case TurnState.Deploy:
                _confirmButton.interactable = nbSelected == _nbCardsToDeploy;
                break;
        }
    }

    public void ResetPanelUI(){
        _interaction.SetActive(false);
        _waitingText.SetActive(false);
        _skipButton.SetActive(false);
        _buttons.SetActive(true);
        Close();
    }

    public void OnCloseButtonPressed() => Close();

    public void Open() => _container.SetActive(true);
    public void Close() => _container.SetActive(false);
}