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
    [SerializeField] private GameObject _cards;

    private void Awake()
    {
        DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
        _cardUI = gameObject.GetComponent<CardUI>();
        _cards = GameObject.Find("PlayerHand");
    }

    private void StartDiscardPhase(){
        _discardPanel = DiscardPanel.Instance;
        _isSelected = false;
    }

    public void OnDiscardClick(){

        if (!(transform.IsChildOf(_cards.transform))) return; // Return if card not in hand
        if (!_discardPanel) return; // Return if it's not discard phase

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
