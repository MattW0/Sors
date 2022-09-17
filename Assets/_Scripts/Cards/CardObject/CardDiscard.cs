using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardDiscard : MonoBehaviour
{
    private DiscardPanel _discardPanel;

    private bool _isSelected;
    private Vector2 _startPosition;
    private CardUI _cardUI;
    [SerializeField] private GameObject playerHand;

    private void Awake()
    {
        DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
        _cardUI = gameObject.GetComponent<CardUI>();
        playerHand = GameObject.Find("PlayerHand");
    }

    private void StartDiscardPhase(){
        _discardPanel = DiscardPanel.Instance;
        _isSelected = false;
    }

    public void OnDiscardClick(){

        if (!(transform.IsChildOf(playerHand.transform))) return; // Return if card not in hand
        if (!_discardPanel) return; // Return if it's not discard phase

        var trans = transform;
        
        if (_isSelected) {
            _isSelected = false;
            _cardUI.Highlight(false);
            transform.position = _startPosition;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _cardUI.Highlight(true);
        _startPosition = trans.position;
        trans.position = _startPosition + Vector2.up * 10;
        _discardPanel.CardToDiscardSelected(gameObject, true);
    }

    private void OnDestroy() {
        DiscardPanel.OnDiscardPhaseStarted -= StartDiscardPhase;
    }
}
