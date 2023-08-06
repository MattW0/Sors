using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;


public class CardStats : NetworkBehaviour, IPointerClickHandler
{
    public PlayerManager owner { get; private set; }
    public CardInfo cardInfo;
    private CardUI _cardUI;
    private CardZoomView _cardZoomView;

    private bool _interactable;
    public bool IsInteractable { 
        get => _interactable;
        set {
            _interactable = value;
            _cardUI.Highlight(value, ColorManager.interactionHighlight);
        }
    }

    private bool _discardable;
    public bool IsDiscardable { 
        get => _discardable;
        set {
            _discardable = value;
            _cardUI.Highlight(value, ColorManager.discardHighlight);
        }
    }

    private bool _trashable;
    public bool IsTrashable { 
        get => _trashable;
        set {
            _trashable = value;
            _cardUI.Highlight(value, ColorManager.trashHighlight);
        }
    }
    
    private void Awake()
    {
        var networkIdentity = NetworkClient.connection.identity;
        owner = networkIdentity.GetComponent<PlayerManager>();
        
        _cardUI = gameObject.GetComponent<CardUI>();
        _cardZoomView = CardZoomView.Instance;
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        cardInfo = card;
        _cardUI.SetCardUI(card);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _cardZoomView.ZoomCard(cardInfo);
        }
    }

    public void SetHighlight() => _cardUI.Highlight(IsInteractable, ColorManager.interactionHighlight);
}
