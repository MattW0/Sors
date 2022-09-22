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
    private CardStats _stats;

    private void Awake()
    {
        // DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
        _cardUI = gameObject.GetComponent<CardUI>();
        playerHand = GameObject.Find("PlayerHand");

        _stats = gameObject.GetComponent<CardStats>();
        if(!_discardPanel) _discardPanel = DiscardPanel.Instance;
    }

    // private void StartDiscardPhase(){
    //     _discardPanel = DiscardPanel.Instance;
    //     _isSelected = false;
    // }

    public void OnDiscardClick(){
        // if (!transform.IsChildOf(playerHand.transform)) return; // Return if card not in hand
        // if (!_discardPanel) return; // Return if it's not discard phase
        if (!_stats.isDiscardable) return;
        
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

    // private void OnDestroy() {
    //     DiscardPanel.OnDiscardPhaseStarted -= StartDiscardPhase;
    // }
}
