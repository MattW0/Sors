using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandInteractionUI : MonoBehaviour
{   
    [Header("Entities")]
    [SerializeField] private HandInteractionPanel _handInteractionPanel;
    private PlayerManager _localPlayer;

    [Header("UI")]
    [SerializeField] private static GameObject _view;
    [SerializeField] private Transform _playerHand;
    private Vector3 _handPositionStandard = Vector3.zero;
    private Vector3 _handPositionPlayCards = new Vector3(-180, 0, 0);
    [SerializeField] private Image _fullViewImage;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private GameObject _skipButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private TMP_Text _displayText;

    [Header("Helper Fields")]
    private TurnState _state;
    private int _nbCardsToDiscard;
    private int _nbCardsToPlay = 1;
    private int _nbCardsToSelectMax;
    
    public void PrepareCardCollectionPanelUi(int nbCardsToDiscard){
        _localPlayer = PlayerManager.GetLocalPlayer();

        _nbCardsToDiscard = nbCardsToDiscard;

        _view = gameObject.transform.GetChild(0).gameObject;
        // _interaction.SetActive(false);
        _buttons.SetActive(false);
        _skipButton.SetActive(false);

        _waitingText.SetActive(false);
        _displayText.text = "";
    }

    public void BeginCardIntoHand(int nbCardsToSelect){
        _nbCardsToSelectMax = nbCardsToSelect;

        if (_nbCardsToSelectMax == 0) SkipInteraction(TurnState.CardIntoHand);
        else InteractionBegin(TurnState.CardIntoHand);
    }

    public void BeginTrash(int nbCardsToTrash){
        _nbCardsToSelectMax = nbCardsToTrash;

        if (nbCardsToTrash == 0) SkipInteraction(TurnState.Trash);
        else InteractionBegin(TurnState.Trash);
    }

    public void InteractionBegin(TurnState state){
        _view.SetActive(true);
        StartHandInteractionUI();

        _state = state;
        switch (_state){
            case TurnState.Develop or TurnState.Deploy:
                _collectionTitle.text = "Hand";
                PlayCardInteraction();
                break;
            case TurnState.Discard:
                _collectionTitle.text = "Hand";
                _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
                _fullViewImage.enabled = true;
                _closeButton.interactable = false;
                break;
            case TurnState.CardIntoHand:
                _collectionTitle.text = "Discard";
                _displayText.text = $"Put up to {_nbCardsToSelectMax} card(s) into your hand";
                _confirmButton.interactable = true;
                _fullViewImage.enabled = true;
                break;
            case TurnState.Trash:
                _collectionTitle.text = "Hand";
                _displayText.text = $"Trash up to {_nbCardsToSelectMax} card(s)";
                _confirmButton.interactable = true;
                _fullViewImage.enabled = true;
                break;
        }
    }

    private void PlayCardInteraction(){

        _playerHand.localPosition = _handPositionPlayCards;

        int playerActionsLeft = 0;
        string displayText = "";
        if (_state == TurnState.Develop) {
            playerActionsLeft = _localPlayer.Develops;
            displayText = $"You may develop a card";
        }
        else if(_state == TurnState.Deploy) {
            playerActionsLeft = _localPlayer.Deploys;
            displayText = $"You may deploy a card";
        }

        if(playerActionsLeft <= 0){
            _buttons.SetActive(false);
            _waitingText.SetActive(true);
            _displayText.text = $"You can't play more cards";
        } else {
            _skipButton.SetActive(true);
            _displayText.text = displayText;
        }
    }

    public void OnConfirmButtonPressed(){

        if (_state == TurnState.Discard) _handInteractionPanel.ConfirmDiscard();
        else if (_state == TurnState.CardIntoHand || _state == TurnState.Trash) 
            _handInteractionPanel.ConfirmPrevailCardSelection();
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) 
            _handInteractionPanel.ConfirmPlay();

        _buttons.SetActive(false);
        _waitingText.SetActive(true);
        _closeButton.interactable = true;
    }

    public void OnSkipButtonPressed() => SkipInteraction(_state);
    public void SkipInteraction(TurnState state){
        _buttons.SetActive(false);
        _waitingText.SetActive(true);

        if(_state == TurnState.Develop || _state == TurnState.Deploy) 
            _localPlayer.CmdSkipCardPlay();
        else if(state == TurnState.CardIntoHand || state == TurnState.Trash) 
            _localPlayer.CmdPlayerSkipsPrevailOption();
    }

    public void UpdateInteractionElements(int nbSelected){
        switch (_state){
            case TurnState.Discard:
                _displayText.text = $"Discard {nbSelected}/{_nbCardsToDiscard} cards";
                _confirmButton.interactable = nbSelected == _nbCardsToDiscard;
                break;
            case TurnState.Develop or TurnState.Deploy:
                _confirmButton.interactable = nbSelected == _nbCardsToPlay;
                break;
        }
    }

    private void StartHandInteractionUI(){
        _buttons.SetActive(true);
        // _interaction.SetActive(true);
        _waitingText.SetActive(false);
        _confirmButton.interactable = false;
        _collectionTitle.text = "Hand";
    }

    public void ResetPanelUI(bool hard){
        _fullViewImage.enabled = false;
        _buttons.SetActive(true);
        _waitingText.SetActive(false);
        if(!hard) return;
        
        // _interaction.SetActive(false);
        _playerHand.localPosition = _handPositionStandard;
        _skipButton.SetActive(false);
        Close();
    }

    public void OnCloseButtonPressed() => Close();
    public void ToggleView() {
        if(_view.activeSelf) Close();
        else Open();
    }
    public static void Open() => _view.SetActive(true);
    public static void Close() => _view.SetActive(false);

}