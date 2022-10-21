using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayZoneCardHolder : MonoBehaviour, IDropHandler
{
    public static event Action<GameObject, int> OnCardDeployed;
    [SerializeField] private Image highlight;
    [SerializeField] private BoxCollider2D boxCollider2D;

    private bool _containsCard;

    private void Awake()
    {
        highlight.enabled = false;
    }

    public void Highlight(bool active)
    {
        if (_containsCard)
        {
            highlight.enabled = false;
            return;
        }
        
        highlight.enabled = active;
    }

    public void OnDrop(PointerEventData data)
    {
        if (!boxCollider2D.enabled) return;
        
        // gets the holder object name (0, 1, ..) to know position
        var cardObject = data.pointerPress;
        int.TryParse(gameObject.name, out var holderNumber);

        PlaceCard();

        OnCardDeployed?.Invoke(cardObject, holderNumber - 1);
        EntityManager.PlayerDeployCard(cardObject, holderNumber - 1);
    }

    private void PlaceCard()
    {
        _containsCard = true;
        boxCollider2D.enabled = false;
        Highlight(false);
    }
}
