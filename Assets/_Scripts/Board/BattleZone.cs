using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    [SerializeField] private PlayZoneManager playZoneManager; 
    [SerializeField] private List<PlayZoneCardHolder> cardHolders;
    
    private Dictionary<int, BattleZoneEntity> _battleZoneCards;
    public List<BattleZoneEntity> GetEntities => _battleZoneCards.Values.ToList();

    private void Awake()
    {
        PlayZoneCardHolder.OnCardDeployed += HandleCardDeployed;
        _battleZoneCards = new Dictionary<int, BattleZoneEntity>();
    }

    private void HandleCardDeployed(GameObject card, int holderNumber)
    {
        if (!playZoneManager.isMyZone) return;
        
        var fieldEntity = card.GetComponent<BattleZoneEntity>();
        fieldEntity.IsDeployed = true;
        _battleZoneCards.Add(holderNumber, fieldEntity);

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
