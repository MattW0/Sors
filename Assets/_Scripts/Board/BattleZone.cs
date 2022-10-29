using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private List<PlayZoneCardHolder> cardHolders;

    public void Highlights(bool active)
    {
        if (active) HighlightCardHolders();
        else ResetHighlights();
    }
    
    private void HighlightCardHolders()
    {
        foreach (var holder in cardHolders) {
            holder.Highlight();
        }
    }

    public void ResetHighlights()
    {
        foreach (var holder in cardHolders) {
            holder.ResetHighlight();
        }
    }
}
