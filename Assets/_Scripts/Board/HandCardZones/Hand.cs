using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public static Hand Instance { get; private set; }
    private List<CardStats> _handCards = new();
    public List<CardStats> GetHandCards => _handCards;

    private void Awake(){
        if (!Instance) Instance = this;
    }

    public void UpdateHandsCardList(GameObject card, bool addingCard){
        var stats = card.GetComponent<CardStats>();
        
        // print($"Updating hand cards list : {stats.cardInfo.title} , added : {addingCard}");
        
        if (addingCard) _handCards.Add(stats);
        else _handCards.Remove(stats);
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) => HighlightMoney(isInteractable);
    
    [TargetRpc]
    public void TargetHighlightMoney(NetworkConnection target) => HighlightMoney(true);

    public void HighlightMoney(bool b){
        foreach (var card in _handCards.Where(card => card.cardInfo.type == CardType.Money)) {
            card.IsInteractable = b;
        }
    }
}
