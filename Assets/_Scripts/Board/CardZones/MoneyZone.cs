using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MoneyZone : MonoBehaviour
{
    [SerializeField] private bool myZone;
    private CardMover _cardMover;

    private void Start(){
        _cardMover = CardMover.Instance;
    }

    public void DiscardMoney() => _cardMover.MoveAllTo(GetCards(), myZone, CardLocation.MoneyZone, CardLocation.Discard);

    private List<GameObject> GetCards()
    {
        var cards = new List<GameObject>();
        foreach (Transform child in transform)
        {
            cards.Add(child.gameObject);
        }
        return cards;
    }
}
