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
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private DetailCardPreview _detailCardPreview;

    [Header("Interaction States")]
    private InteractionStateBase _state;

    [Header("Helper Fields")]
    private bool _isWaiting;

    private void Start()
    {
        _skipButton.onClick.AddListener(SkipState);
        _resetButton.onClick.AddListener(ResetState);
        _confirmButton.onClick.AddListener(ConfirmState);

        _displayText.text = "";
        _detailCardPreview.HideAll(true);
    }

    public void StartInteraction(InteractionStateBase state, int nbCardsToSelectMax, bool autoSkip)
    {
        print("Interaction begin " + state + ", " + nbCardsToSelectMax + ", " + autoSkip);
        _state = state;

        if(autoSkip){
            SkipState();
            return;
        }

        _displayText.text = _state.InteractionText;
        _isWaiting = false;

        SetPanelButtons();
        PanelIn();
    }

    private void SetPanelButtons()
    {
        // Confirm button is always enabled
        _confirmButton.interactable = _state.config.confirmButtonEnabled;

        _resetButton.gameObject.SetActive(_state.config.resetButtonVisible);
        _resetButton.interactable = _state.config.resetButtonEnabled;

        _skipButton.gameObject.SetActive(_state.config.skipButtonVisible);
        _skipButton.interactable = _state.config.skipButtonEnabled;
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

    private void SkipState() 
    {
        _state.OnSkip();
        Wait();
    }

    private void ResetState() 
    {
        _state.OnReset();
        Wait();
    }

    private void ConfirmState() 
    {
        _state.OnConfirm();
        Wait();
    }

    private void Wait()
    {
        _isWaiting = true;
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _displayText.text = "Wait for opponent...";
    }

    private void OnDestroy()
    {
        // InteractionPanel.OnInteractionBegin -= InteractionBegin;
    }
}