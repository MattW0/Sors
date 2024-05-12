using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private GameObject _interactionView;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private GameObject _skipButtonGameObject;
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _displayText;

    [Header("Helper Fields")]
    private TurnState _state;
    private int _nbCardsToDiscard;
    private int _nbCardsToPlay = 1;
    private int _nbCardsToSelectMax;
    private InteractionPanel _interactionPanel;

    private void Start()
    {
        _interactionPanel = InteractionPanel.Instance;

        _skipButton.onClick.AddListener(OnSkipButtonPressed);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
    }
    
    public void PrepareInteractionPanel(int nbCardsToDiscard)
    {
        _nbCardsToDiscard = nbCardsToDiscard;

        _interactionView.SetActive(false);
        _buttons.SetActive(false);
        _skipButtonGameObject.SetActive(false);

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

    public void InteractionBegin(TurnState state, int numberPlays = 0)
    {
        _interactionView.SetActive(true);
        StartInteractionUI();

        _state = state;
        switch (_state){
            case TurnState.Develop or TurnState.Deploy:
                PlayCardInteraction(numberPlays);
                break;
            case TurnState.Discard:
                _displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
                break;
            case TurnState.CardIntoHand:
                _displayText.text = $"Put up to {_nbCardsToSelectMax} card(s) into your hand";
                _confirmButton.interactable = true;
                break;
            case TurnState.Trash:
                _displayText.text = $"Trash up to {_nbCardsToSelectMax} card(s)";
                _confirmButton.interactable = true;
                break;
        }
    }

    private void PlayCardInteraction(int numberPlays)
    {
        if(numberPlays <= 0){
            _buttons.SetActive(false);
            _waitingText.SetActive(true);
            _displayText.text = $"You can't play more cards";
        } else {
            _skipButtonGameObject.SetActive(true);
            if (_state == TurnState.Develop) {
                _displayText.text = $"You may develop a card";
            }
            else if(_state == TurnState.Deploy) {
                _displayText.text = $"You may deploy a card";
            }
        }
    }

    public void OnConfirmButtonPressed()
    {
        if (_state == TurnState.Discard) _interactionPanel.ConfirmDiscard();
        else if (_state == TurnState.CardIntoHand || _state == TurnState.Trash) 
            _interactionPanel.ConfirmPrevailCardSelection();
        else if (_state == TurnState.Develop || _state == TurnState.Deploy) 
            _interactionPanel.ConfirmPlay();

        _buttons.SetActive(false);
        _waitingText.SetActive(true);
    }

    public void OnSkipButtonPressed() => SkipInteraction(_state);
    public void SkipInteraction(TurnState state){
        _buttons.SetActive(false);
        _waitingText.SetActive(true);

        if(_state == TurnState.Develop || _state == TurnState.Deploy) 
            _interactionPanel.SkipCardPlay();
        else if(state == TurnState.CardIntoHand || state == TurnState.Trash) 
            _interactionPanel.PlayerSkipsPrevailOption();
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

    private void StartInteractionUI(){
        _buttons.SetActive(true);
        // _interaction.SetActive(true);
        _waitingText.SetActive(false);
        _confirmButton.interactable = false;
    }

    public void ResetPanelUI(bool hard){
        _buttons.SetActive(true);
        _waitingText.SetActive(false);
        if(!hard) return;
        
        _interactionView.SetActive(false);
        _skipButtonGameObject.SetActive(false);
        // Close(); TODO
    }
}