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
    [SerializeField] private DetailCardPreview _detailCardPreview;
    

    [Header("Helper Fields")]
    private TurnState _state;
    private int _nbCardsToSelectMax;
    private CardSelectionHandler _selectionHandler;
    private bool _isWaiting;

    private void Start()
    {
        _selectionHandler = GetComponentInParent<CardSelectionHandler>();
        
        _skipButton.onClick.AddListener(OnSkipButtonPressed);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);

        _displayText.text = "";
        _detailCardPreview.HideAll(true);

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
        _isWaiting = false;
        
        _confirmButton.interactable = false;
        _skipButton.interactable = state != TurnState.Discard;
        _displayText.text = GetInteractionString();

        PanelIn();
    }

    private void OnConfirmButtonPressed()
    {
        Wait();
        _selectionHandler.ConfirmSelection();
    }
    private void OnSkipButtonPressed() => SkipInteraction();
    private void SkipInteraction()
    {   
        Wait();
        _selectionHandler.SkipInteraction();
    }
    internal void SetConfirmButtonEnabled(bool b) => _confirmButton.interactable = b;

    public void SelectMarketTile(CardInfo cardInfo)
    {
        _detailCardPreview.ShowPreview(cardInfo, cardInfo.type != CardType.Money);

        if (_isWaiting) return;
        _confirmButton.interactable = true;
    }

    public void DeselectMarketTile()
    {
        _detailCardPreview.HideAll(true);
        _confirmButton.interactable = false;
    }

    private string GetInteractionString()
    {
        if (_state == TurnState.Discard) return $"Discard {_nbCardsToSelectMax} card(s)";
        if (_state == TurnState.CardSelection || _state == TurnState.Trash) 
        {
            // "Up to X cards"
            _confirmButton.interactable = true;
            if (_state == TurnState.CardSelection) return $"Put up to {_nbCardsToSelectMax} card(s) into your hand";
            if (_state == TurnState.Trash) return $"Trash up to {_nbCardsToSelectMax} card(s)";
        }

        // Buy, play, select
        return $"You may {InteractionActionVerb()} a {CardTypeString()} card";
    }
    private void Wait()
    {
        _isWaiting = true;
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _displayText.text = "Wait for opponent...";
    }

    private string CardTypeString()
    {
        var cardTypes = _state switch {
            TurnState.Invent or TurnState.Develop => "Technology",
            TurnState.Recruit or TurnState.Deploy => "Creature",
            _ => ""
        };
        if (_state == TurnState.Invent || _state == TurnState.Recruit) cardTypes += " or Money";
        return cardTypes;
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

    private void OnDestroy()
    {
        InteractionPanel.OnInteractionBegin -= InteractionBegin;
    }
}