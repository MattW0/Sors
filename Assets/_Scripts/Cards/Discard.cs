using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Discard : NetworkBehaviour
{
    private DiscardPanel _discardPanel;

    private bool _isSelected;
    private Vector2 _startPosition;

    private void Awake()
    {
        DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
    }

    private void StartDiscardPhase(){
        _discardPanel = DiscardPanel.Instance;
        _isSelected = false;
    }

    public void OnDiscardClick(){
        if (!hasAuthority || !_discardPanel) return;

        if (_isSelected) {
            transform.position = _startPosition;
            _isSelected = false;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _startPosition = transform.position;
        transform.position = _startPosition + Vector2.up * 10;
        _discardPanel.CardToDiscardSelected(gameObject, true);
    }

    private void OnDestroy() {
        DiscardPanel.OnDiscardPhaseStarted -= StartDiscardPhase;
    }
}
