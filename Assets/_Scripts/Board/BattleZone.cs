using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private List<PlayZoneCardHolder> cardHolders;

    public int GetNbCardHolders() => cardHolders.Count;

    public void HighlightCardHolders(IEnumerable<int> indexes, bool active)
    {
        foreach (var i in indexes)
        {
            cardHolders[i].Highlight(active);
        }
    }

    public void ResetHighlight()
    {
        foreach (var ch in cardHolders)
        {
            ch.Highlight(false);
        }
    }
}
