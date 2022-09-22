using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerHandManager : NetworkBehaviour
{
    public static PlayerHandManager Instance { get; private set; }

    private void Awake()
    {
        if (!Instance) Instance = this;
    }
    
    [ClientRpc]
    public void RpcHighlightAll(bool isInteractable) {
        foreach (Transform child in transform) {
            var card = child.gameObject.GetComponent<CardStats>();
            
            // if (card.cardInfo.isCreature) continue;
            card.SetDiscardable(isInteractable);
        }
    }

    [ClientRpc]
    public void RpcHighlightMoney(bool isInteractable) {
        foreach (Transform child in transform) {
            var card = child.gameObject.GetComponent<CardStats>();
            
            if (card.cardInfo.isCreature) continue;
            card.SetInteractable(isInteractable);
        }
    }
}
