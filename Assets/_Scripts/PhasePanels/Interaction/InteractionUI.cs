using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : AnimatedPanel
{
    [Header("UI")]
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _displayText;

    [Header("Market Selection")]
    [SerializeField] private GameObject _creatureCard;
    [SerializeField] private GameObject _technologyCard;
    [SerializeField] private GameObject _moneyCard;

    [Header("Helper Fields")]
    private TurnState _state;
    private int _nbCardsToSelectMax;
    private CardSelectionHandler _selectionHandler;

    private void Start()
    {
        _selectionHandler = GetComponentInParent<CardSelectionHandler>();
        
        _skipButton.onClick.AddListener(OnSkipButtonPressed);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);

        _displayText.text = "";
        
        _creatureCard.SetActive(false);
        _technologyCard.SetActive(false);
        _moneyCard.SetActive(false);

        InteractionPanel.OnInteractionBegin += InteractionBegin;
    }

    public void InteractionBegin(TurnState state, int nbCardsToSelectMax, bool autoSkip)
    {
        if(autoSkip){
            SkipInteraction();
            return;
        }

        _state = state;
        _nbCardsToSelectMax = nbCardsToSelectMax;
        _confirmButton.interactable = false;
        _skipButton.interactable = state != TurnState.Discard;

        var actionVerb = InteractionActionVerb();
        PanelIn();

        if (_state == TurnState.Discard) _displayText.text = $"Discard {_nbCardsToSelectMax} card(s)";
        else if (_state == TurnState.CardSelection || _state == TurnState.Trash) PrevailInteraction();
        else MoneyInteraction(actionVerb);
    }

    private void MoneyInteraction(string actionVerb)
    {
        var cardType = _state switch {
            TurnState.Invent or TurnState.Develop => " Technology",
            TurnState.Recruit or TurnState.Deploy => " Creature",
            _ => ""
        };

        if (_state == TurnState.Invent || _state == TurnState.Recruit) cardType += " or Money";
        
        _displayText.text = $"You may {actionVerb} a{cardType} card";
    }

    private void PrevailInteraction()
    {
        // "Up to X cards"
        _confirmButton.interactable = true;

        if (_state == TurnState.CardSelection)
            _displayText.text = $"Put up to {_nbCardsToSelectMax} card(s) into your hand";
        else if (_state == TurnState.Trash)
            _displayText.text = $"Trash up to {_nbCardsToSelectMax} card(s)";
    }

    public void SelectMarketTile(CardInfo cardInfo)
    {
        var previewCardObject = cardInfo.type switch{
            CardType.Creature => _creatureCard,
            CardType.Technology => _technologyCard,
            CardType.Money => _moneyCard,
            _ => null
        };

        var detailCard = previewCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);
        previewCardObject.SetActive(true);

        _confirmButton.interactable = true;
    }

    public void DeselectMarketTile()
    {
        _creatureCard.SetActive(false);
        _technologyCard.SetActive(false);
        _moneyCard.SetActive(false);

        _confirmButton.interactable = false;
    }

    private string InteractionActionVerb()
    {
        var actionVerb = _state switch{
            TurnState.Discard => "discard",
            TurnState.Trash => "trash",
            TurnState.Invent or TurnState.Recruit => "buy",
            TurnState.Develop or TurnState.Deploy => "play",
            _ => "select"
        };

        return actionVerb;
    }
    private void OnConfirmButtonPressed()
    {
        _displayText.text = "Wait for opponent...";
        _confirmButton.interactable = false;
        _selectionHandler.ConfirmSelection();   
    }
    private void OnSkipButtonPressed() => SkipInteraction();
    private void SkipInteraction()
    {   
        _displayText.text = "Wait for opponent...";
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _selectionHandler.OnSkipInteraction();
    }
    internal void SetConfirmButtonEnabled(bool b) => _confirmButton.interactable = b;
    private void OnDestroy()
    {
        InteractionPanel.OnInteractionBegin -= InteractionBegin;
    }
}