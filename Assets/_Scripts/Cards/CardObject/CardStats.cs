using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CardStats : NetworkBehaviour
{
    public PlayerManager owner;
    public CardInfo cardInfo;
    public bool isInteractable;

    private CardUI _cardUI;

    private void Awake()
    {
        var networkIdentity = NetworkClient.connection.identity;
        owner = networkIdentity.GetComponent<PlayerManager>();
        
        _cardUI = gameObject.GetComponent<CardUI>();
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        _cardUI.Highlight(isInteractable);
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        this.cardInfo = card;
        _cardUI.SetCardUI(card);
    }
}