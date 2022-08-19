using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Discard : NetworkBehaviour
{
    private DiscardPanel _discardPanel;

    private bool _isSelected;
    private Vector2 _startPosition;
    private Image _highlight;

    private void Awake()
    {
        DiscardPanel.OnDiscardPhaseStarted += StartDiscardPhase;
        _highlight = gameObject.transform.GetChild(0).GetComponent<Image>();
    }

    private void StartDiscardPhase(){
        _discardPanel = DiscardPanel.Instance;
        _isSelected = false;
    }

    public void OnDiscardClick(){
        if (!hasAuthority || !_discardPanel) return;

        if (_isSelected) {
            _isSelected = false;
            _highlight.enabled = false;
            transform.position = _startPosition;
            _discardPanel.CardToDiscardSelected(gameObject, false);
            return;
        }

        _isSelected = true;
        _highlight.enabled = true;
        _startPosition = transform.position;
        transform.position = _startPosition + Vector2.up * 10;
        _discardPanel.CardToDiscardSelected(gameObject, true);
    }

    private void OnDestroy() {
        DiscardPanel.OnDiscardPhaseStarted -= StartDiscardPhase;
    }
}
