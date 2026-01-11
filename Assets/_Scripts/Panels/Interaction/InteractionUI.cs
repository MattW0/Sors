using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : AnimatedPanel
{
    [Header("UI")]
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private DetailCardPreview _detailCardPreview;

    [Header("Helper Fields")]
    private bool _isWaiting;
    private InteractionPanel _panel;

    private void Start()
    {
        _panel = GetComponentInParent<InteractionPanel>(); 

        _confirmButton.onClick.AddListener(Confirm);
        _skipButton.onClick.AddListener(Skip);
        _resetButton.onClick.AddListener(Reset);

        _displayText.text = "";
        _detailCardPreview.HideAll();
    }

    public void StartInteraction(InteractionStateBase state, bool skip)
    {
        _displayText.text = state.InteractionText;
        _isWaiting = false;

        SetPanelButtons(state);
        PanelIn();
        
        if(skip) Skip();
    }

    private void SetPanelButtons(InteractionStateBase state)
    {
        // Confirm button is always enabled
        _confirmButton.interactable = state.IsConfirmEnabled(numberSelected: 0);

        _skipButton.gameObject.SetActive(state.Config.skipButtonVisible);
        _skipButton.interactable = state.Config.skipButtonEnabled;

        _resetButton.gameObject.SetActive(state.Config.resetButtonVisible);
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
        _detailCardPreview.HideAll();
        _confirmButton.interactable = false;
    }

    private void Confirm() 
    {
        _panel.ConfirmCurrentState();
        Wait();
    }

    private void Skip() 
    {
        _panel.SkipCurrentState();
        Wait();
    }

    private void Reset() 
    {
        _panel.ResetCurrentState();
    }

    private void Wait()
    {
        _isWaiting = true;
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _displayText.text = "Wait for opponent...";
    }
}