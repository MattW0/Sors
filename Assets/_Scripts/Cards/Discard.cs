using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Discard : NetworkBehaviour
{
    private DiscardPanel _discardPanel;

    private bool _isSelected;
    private Vector2 _startPosition;
    private CardUI _cardUI;

    private void Awake()
    {
        DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    private void StartDiscardPhase(){
        _discardPanel = DiscardPanel.Instance;
        _isSelected = false;
    }

    public void OnDiscardClick(){
        // Return if it's not discard phase or card is not in hand
        if (!_discardPanel) return;
        if (!(gameObject.transform.parent.name == "PlayerHand")) return; // Kinda ugly, might need to change

        if (_isSelected) {
            _isSelected = false;
            _cardUI.Highlight(false);
            transform.position = _startPosition;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _cardUI.Highlight(true);
        _startPosition = transform.position;
        transform.position = _startPosition + Vector2.up * 10;
        _discardPanel.CardToDiscardSelected(gameObject, true);
    }

    private void OnDestroy() {
        DiscardPanel.OnDiscardPhaseStarted -= StartDiscardPhase;
    }
}
