using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CardStats : NetworkBehaviour
{
    public CardInfo cardInfo;
    private CardUI _cardUI;

    private void Awake()
    {
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo _cardInfo)
    {
        this.cardInfo = _cardInfo;
        _cardUI.SetCardUI(_cardInfo);
    }
}
