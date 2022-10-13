using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private PlayZoneManager playZoneManager; 
    [SerializeField] private List<PlayZoneCardHolder> cardHolders;
    
    private Dictionary<int, CardStats> _battleZoneCards;
    public List<CardStats> GetCards => _battleZoneCards.Values.ToList();

    private void Awake()
    {
        PlayZoneCardHolder.OnCardDeployed += HandleCardDeployed;
        _battleZoneCards = new Dictionary<int, CardStats>();
    }

    private void HandleCardDeployed(GameObject card, int holderNumber)
    {
        if (!playZoneManager.isMyZone) return;
        
        var cardStats = card.GetComponent<CardStats>();
        cardStats.IsDeployable = false;
        _battleZoneCards.Add(holderNumber, cardStats);

        PlayZoneManager.DeployCard(card, holderNumber);
    }

    public void HighlightCardHolders()
    {
        foreach (var holder in cardHolders) {
            holder.Highlight(true);
        }
    }

    public void ResetHighlight()
    {
        foreach (var holder in cardHolders) {
            holder.Highlight(false);
        }
    }
}
