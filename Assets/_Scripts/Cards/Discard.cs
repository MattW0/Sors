using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Discard : NetworkBehaviour
{
    private DiscardPanel discardPanel; 
    private bool _isSelected;
    private Vector2 _startPosition;

    private void Update()
    {
        discardPanel = DiscardPanel.instance;
    }

    public void OnDiscardClick(){
        if (!hasAuthority || !discardPanel) return;

        print("Discarding");

        if (_isSelected) {
            transform.position = _startPosition;
            _isSelected = false;
            discardPanel.CardToDiscardSelected(false);
            return;
        }

        _isSelected = true;
        _startPosition = transform.position;
        transform.position = _startPosition + Vector2.up * 10;
        discardPanel.CardToDiscardSelected(true);
    }
}
