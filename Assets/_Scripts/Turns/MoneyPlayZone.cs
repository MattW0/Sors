using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPlayZone : MonoBehaviour
{
    public void DiscardMoney(bool auth)
    {
        var children = new List<CardMover>(); 
        children.AddRange(GetComponentsInChildren<CardMover>());

        foreach (var obj in children) { 
            obj.MoveToDestination(auth, CardLocations.Discard);
        }
    }

    public List<GameObject> GetCards()
    {
        var cards = new List<GameObject>();
        foreach (Transform child in transform)
        {
            cards.Add(child.gameObject);
        }
        return cards;
    }
}
