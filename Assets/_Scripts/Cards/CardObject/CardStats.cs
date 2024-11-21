using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(HandCardUI))]
public class CardStats : NetworkBehaviour
{
    public CardInfo cardInfo;
    private HandCardUI _cardUI;

    private bool _interactable;
    public bool IsInteractable {
        get => _interactable;
        set {
            _interactable = value;
            _cardUI.Highlight(HighlightType.InteractionPositive);
        }
    }

    private bool _selected;
    public bool IsSelected {
        get => _selected;
        set {
            _selected = value;
        }
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
        _cardUI.SetCardUI(card);
    }

    public void CheckPlayability(int cash)
    {
        if (cash < cardInfo.cost) return;

        IsInteractable = true;
        _cardUI.Highlight(HighlightType.InteractionPositive);
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
