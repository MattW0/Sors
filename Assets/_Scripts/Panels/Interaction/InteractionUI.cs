using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InteractionUI : AnimatedPanel
{
    [Header("UI")]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private DetailCardPreview _detailCardPreview;

    [Header("Interaction States")]
    private InteractionStateBase _state;

    [Header("Helper Fields")]
    private bool _isWaiting;

    private void Start()
    {
        _confirmButton.onClick.AddListener(Confirm);
        _skipButton.onClick.AddListener(Skip);
        _resetButton.onClick.AddListener(Reset);

        _displayText.text = "";
        _detailCardPreview.HideAll(true);
    }

    public void StartInteraction(IInteractionState state, bool skip, int nbCardsToSelectMax = -1)
    {
        // print("Interaction begin " + state + ", " + nbCardsToSelectMax);
        _state = (InteractionStateBase) state;

        _displayText.text = _state.InteractionText;
        _isWaiting = false;

        SetPanelButtons();
        PanelIn();
        
        // TODO: Should panel always fade in? To better make player understand what is going on
        if(skip) Skip();
    }

    private void SetPanelButtons()
    {
        // Confirm button is always enabled
        _confirmButton.interactable = _state.Config.confirmButtonEnabled;

        _skipButton.gameObject.SetActive(_state.Config.skipButtonVisible);
        _skipButton.interactable = _state.Config.skipButtonEnabled;

        _resetButton.gameObject.SetActive(_state.Config.resetButtonVisible);
        // Reset button is always enabled
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

    private void Confirm() 
    {
        _state.OnConfirm();
        Wait();
    }

    private void Skip() 
    {
        _state.OnSkip();
        Wait();
    }

    private void Reset() 
    {
        _state.OnReset();
    }

    private void Wait()
    {
        _isWaiting = true;
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _displayText.text = "Wait for opponent...";
    }
}