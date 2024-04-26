using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleZoneEntityUI : EntityUI, IPointerClickHandler
{
    private CardZoomView _cardZoomView;
    public void OnPointerClick(PointerEventData eventData) {
        // Right click to zoom only
        if (eventData.button != PointerEventData.InputButton.Right) return;
        
        _cardZoomView.ZoomCard(CardInfo);
    }

    private void Start(){
        _cardZoomView = CardZoomView.Instance;
    }
}
