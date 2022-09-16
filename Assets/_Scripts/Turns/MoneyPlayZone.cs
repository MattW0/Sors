using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPlayZone : MonoBehaviour
{
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
