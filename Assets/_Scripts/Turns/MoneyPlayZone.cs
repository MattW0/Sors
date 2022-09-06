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
        print($"Found {children.Count} children");

        foreach (var obj in children)
        {
            obj.MoveToDestination(auth, CardLocations.Discard);
        }
    }

}
