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

    public void StartDiscard(bool isInteractable){
        foreach (var card in _handCards){
            card.IsDiscardable = isInteractable;
        }
    }

    public void StartTrash(bool isInteractable){
        foreach (var card in _handCards){
            card.IsTrashable = isInteractable;
        }
    }

    [TargetRpc]
    public void TargetTrashCard(NetworkConnection conn, CardStats stats){
        print("Remove card from hand: " + stats.cardInfo.title);
        _handCards.Remove(stats);
    }

    [Server]
    public void ServerTrashCard(CardStats stats){
        print("Remove card from hand: " + stats.cardInfo.title);
        _handCards.Remove(stats);
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

    private void HighlightMoney(bool b){
        foreach (var card in _handCards.Where(card => !card.cardInfo.isCreature)) card.IsInteractable = b;
    }
    
    [ClientRpc]
    public void RpcResetDeployability(){
        foreach (var card in _handCards) card.IsDeployable = false;
    }

    [TargetRpc]
    public void TargetCheckDeployability(NetworkConnection target, int currentCash){
        foreach (var card in _handCards) card.IsDeployable = (currentCash >= card.cardInfo.cost);
    }

    private void OnDestroy(){
        PlayerManager.OnHandChanged -= UpdateHandsCardList;
    }
}
