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
    [SerializeField] private InteractionState[] interactionStates;
    private InteractionState _currentState;

    [Header("Helper Fields")]
    private int _nbCardsToSelectMax;
    private bool _isWaiting;

    public static event Action<bool> OnSkipInteraction;
    public static event Action<bool> OnResetInteraction;
    public static event Action<bool> OnConfirmInteraction;

    private void Start()
    {        
        _skipButton.onClick.AddListener(OnSkipButtonPressed);
        _resetButton.onClick.AddListener(OnResetButtonPressed);
        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);

        _displayText.text = "";
        _detailCardPreview.HideAll(true);

        InteractionPanel.OnInteractionBegin += InteractionBegin;
    }

    public void InteractionBegin(TurnState state, int nbCardsToSelectMax, bool autoSkip)
    {
        print("Interaction begin " + state + ", " + nbCardsToSelectMax + ", " + autoSkip);
        _nbCardsToSelectMax = nbCardsToSelectMax;
        _isWaiting = false;

        if(autoSkip){
            OnSkipButtonPressed();
            return;
        }

        _currentState = interactionStates.FirstOrDefault(x => x.stateType == state);

        if(_currentState == null){
            Debug.LogError("No interaction state found for " + state);
            return;
        }

        _confirmButton.interactable = _currentState.confirmButtonEnabled;
        _skipButton.interactable = _currentState.skipButtonEnabled;
        _displayText.text = _currentState.GetInteractionString(_nbCardsToSelectMax);

        PanelIn();
    }

    private void OnSkipButtonPressed() 
    {   
        Wait();
        OnSkipInteraction?.Invoke(_nbCardsToSelectMax != -1);
    }
    private void OnResetButtonPressed() => OnResetInteraction?.Invoke(_nbCardsToSelectMax != -1);
    private void OnConfirmButtonPressed()
    {
        Wait();
        OnConfirmInteraction?.Invoke(_nbCardsToSelectMax != -1);
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

    private void Wait()
    {
        _isWaiting = true;
        _confirmButton.interactable = false;
        _skipButton.interactable = false;
        _displayText.text = "Wait for opponent...";
    }

    private void OnDestroy()
    {
        InteractionPanel.OnInteractionBegin -= InteractionBegin;
    }
}