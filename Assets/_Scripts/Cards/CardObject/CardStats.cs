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
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        owner = networkIdentity.GetComponent<PlayerManager>();
        
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    [TargetRpc]
    public void TargetSetInteractable(NetworkConnection target, bool _isInteractable)
    {
        isInteractable = _isInteractable;
        _cardUI.Highlight(isInteractable);
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo _cardInfo)
    {
        this.cardInfo = _cardInfo;
        _cardUI.SetCardUI(_cardInfo);
    }
}
