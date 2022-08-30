using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardRecruit : NetworkBehaviour
{
    private CardStats _cardStats;
    private PlayerManager _owner;

    private void Awake() {
        _cardStats = gameObject.GetComponent<CardStats>();
        _owner = _cardStats.owner;
    }

    public void OnRecruitMoneyPlay(){
        // Return if card can't be played (not in hand or no money card)
        if (!_cardStats.isInteractable) return;
        
        _owner.PlayCard(gameObject);
        _cardStats.isInteractable = false;
        _owner.Cash++;
    }
}
