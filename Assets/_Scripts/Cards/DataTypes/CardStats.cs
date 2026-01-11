using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(HandCardUI))]
public class CardStats : NetworkBehaviour
{
    public CardInfo cardInfo;
    private HandCardUI _cardUI;

    public bool IsSelected { get; set; }
    private bool _isInteractable;
    public bool IsInteractable { 
        get => _isInteractable;
        set {
            _isInteractable = value;
            if(!_isInteractable) _cardUI.DisableHighlight();
        } 
    }
    public CardDragHandler DragHandler { get; internal set; }
    private void Awake()
    {        
        _cardUI = gameObject.GetComponent<HandCardUI>();
        CardSelectionHandler.OnResetCards += ResetCard;
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        // Will be set active by cardMover, once card is spawned correctly in UI
        gameObject.SetActive(false);

        cardInfo = card;
        _cardUI.SetCardUI(card, card.cardSpritePath);
    }

    public void SetInteractable(bool value, Color color)
    {
        IsInteractable = value;
        if(value) _cardUI.SetHighlight(color);
    }

    public void CheckPlayability(int cash) => SetInteractable(cash >= cardInfo.cost, UIManager.ColorPalette.interactionPositiveHighlight);

    private void ResetCard()
    {
        IsInteractable = false;
        IsSelected = false;
        _cardUI.DisableHighlight();

        if(DragHandler == null) return;
        DragHandler.cardVisual.Reset();
    }

    public bool Equals(CardStats other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        return cardInfo.Equals(other.cardInfo);
    }

    private void OnDestroy()
    {
        CardSelectionHandler.OnResetCards -= ResetCard;
    }
}
