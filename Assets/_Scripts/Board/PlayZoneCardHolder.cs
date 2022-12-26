using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayZoneCardHolder : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image highlight;
    [SerializeField] private BoxCollider2D boxCollider2D;

    private bool _containsCard;
    private short _initialChilds;
    
    private void Awake()
    {
        highlight.enabled = false;
        boxCollider2D.enabled = false;
        _initialChilds = (short) transform.childCount;
    }

    public void Highlight()
    {
        highlight.enabled = !_containsCard;
        boxCollider2D.enabled = !_containsCard;
    }

    public void ResetHighlight()
    {
        highlight.enabled = false;

        if (transform.childCount > _initialChilds) return;
        boxCollider2D.enabled = false;
        _containsCard = false;
    }

    public void OnDrop(PointerEventData data)
    {
        if (!boxCollider2D.enabled) return;
        
        // get dropped card and stats 
        var cardObject = data.pointerPress;
        var cardStats = cardObject.GetComponent<CardStats>();
        
        if (!cardStats.IsDeployable) return;
        cardStats.IsDeployable = false;
        
        // gets the holder object name (0, 1, ..) to know position
        int.TryParse(gameObject.name, out var holderNumber);
        PlaceCard();

        PlayerZoneManager.PlayerDeployCard(cardObject, holderNumber - 1);
    }

    private void PlaceCard()
    {
        _containsCard = true;
        highlight.enabled = false;
    }
}
