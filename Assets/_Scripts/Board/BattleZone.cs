using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private EntityManager entityManager; 
    [SerializeField] private List<PlayZoneCardHolder> cardHolders;

    public void Highlight(bool active)
    {
        if (active) HighlightCardHolders();
        else ResetHighlight();
    }
    
    private void HighlightCardHolders()
    {
        foreach (var holder in cardHolders) {
            holder.Highlight(true);
        }
    }

    private void ResetHighlight()
    {
        foreach (var holder in cardHolders) {
            holder.Highlight(false);
        }
    }
}
