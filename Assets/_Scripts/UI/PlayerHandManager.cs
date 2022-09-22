using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerHandManager : NetworkBehaviour
{
    public static PlayerHandManager Instance { get; private set; }
    private List<CardStats> _handCards;

    private void Awake()
    {
        if (!Instance) Instance = this;
        PlayerManager.OnHandChanged += UpdateHandsCardList;
        
        _handCards = new List<CardStats>();
    }

    private void UpdateHandsCardList(GameObject newCard, bool addingCard)
    {
        var stats = newCard.GetComponent<CardStats>();
        if (addingCard) _handCards.Add(stats);
        else _handCards.Remove(stats);
    }
    
    [ClientRpc]
    public void RpcHighlightAll(bool isInteractable) {
        foreach (var card in _handCards) {
            card.SetDiscardable(isInteractable);
        }
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) {
        foreach (var card in _handCards) {
            if (card.cardInfo.isCreature) continue;
            card.SetInteractable(isInteractable);
        }
    }

    private void OnDestroy()
    {
        PlayerManager.OnHandChanged -= UpdateHandsCardList;
    }
}
