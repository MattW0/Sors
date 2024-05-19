using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardStats cardStats;
    private CardZoomView _cardZoomView;
    
    public static event Action<GameObject> OnCardClicked;
    
    private void Start() {
        _cardZoomView = CardZoomView.Instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            _cardZoomView.ZoomCard(cardStats.cardInfo);
            return;
        }

        if(!cardStats.IsInteractable) return;

        OnCardClicked?.Invoke(gameObject);
    }
}
