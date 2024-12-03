using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(HandCardUI))]
public class CardStats : NetworkBehaviour
{
    public CardInfo cardInfo;
    private HandCardUI _cardUI;

    public bool IsSelected { get; set; }
    public bool IsInteractable { get; private set; }
    public void SetInteractable(bool value, TurnState state = TurnState.None)
    {
        IsInteractable = value;
        _cardUI.Highlight(value, state);
    }

    
    private void Awake()
    {        
        _cardUI = gameObject.GetComponent<HandCardUI>();
        CardSelectionHandler.OnInteractionConfirmed += ResetCard;
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        // Will be set active by cardMover, once card is spawned correctly in UI
        gameObject.SetActive(false);

        cardInfo = card;
        _cardUI.SetCardUI(card, card.cardSpritePath);
    }

    public void CheckPlayability(int cash)
    {
        if (cash < cardInfo.cost) return;

        IsInteractable = true;
        _cardUI.Highlight(HighlightType.Playable);
    }

    private void ResetCard()
    {
        IsInteractable = false;
        IsSelected = false;
        _cardUI.Highlight(HighlightType.None);
    }

    public bool Equals(CardStats other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        return cardInfo.Equals(other.cardInfo);
    }

    private void OnDestroy()
    {
        CardSelectionHandler.OnInteractionConfirmed -= ResetCard;
    }
}
