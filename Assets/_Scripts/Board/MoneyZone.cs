using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MoneyZone : MonoBehaviour
{
    [SerializeField] private bool myZone;

    public void DiscardMoney(){
        var cards = GetCards();
        foreach (var card in cards) {
            card.GetComponent<CardMover>().MoveToDestination(myZone, CardLocations.Discard);
        }
    }

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
