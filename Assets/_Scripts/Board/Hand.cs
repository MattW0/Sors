using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public static Hand Instance { get; private set; }
    private List<CardStats> _handCards;
    public List<CardStats> GetHandCards => _handCards;

    private void Awake(){
        if (!Instance) Instance = this;
        PlayerManager.OnHandChanged += UpdateHandsCardList;
        
        _handCards = new List<CardStats>();
    }

    private void UpdateHandsCardList(GameObject card, bool addingCard){
        var stats = card.GetComponent<CardStats>();
        if (addingCard) _handCards.Add(stats);
        else _handCards.Remove(stats);
    }

    public void StartTrash(){
        foreach (var card in _handCards){
            card.IsTrashable = true;
        }
    }
    
    [ClientRpc]
    public void RpcResetHighlight() {
        foreach (var card in _handCards) {
            card.IsDiscardable = false;
            card.IsTrashable = false;
        }
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) => HighlightMoney(isInteractable);
    
    [TargetRpc]
    public void TargetHighlightMoney(NetworkConnection target, bool isInteractable) => HighlightMoney(isInteractable);

    public void HighlightMoney(bool b){
        foreach (var card in _handCards.Where(card => !card.cardInfo.isCreature)) card.IsInteractable = b;
    }

    private void OnDestroy(){
        PlayerManager.OnHandChanged -= UpdateHandsCardList;
    }
}
