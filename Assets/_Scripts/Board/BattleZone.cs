using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleZone : MonoBehaviour
{
    public List<Image> highlights;
    public int GetNbCardHolders() => highlights.Count;

    public void Prepare()
    {
        // foreach (Transform child in transform)
        // {
        //     _highlights.Add(child.GetChild(0).GetComponent<Image>());
        // }
    }

    public void HighlightCardHolders(int[] indexes, bool active)
    {
        foreach (var i in indexes)
        {
            if (i == -1) continue;
            
            highlights[i].enabled = active;
        }
    }
}
