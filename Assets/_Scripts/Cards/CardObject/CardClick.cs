using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardStats cardStats;
    private CardInfo _cardInfo;
    private CardZoomView _cardZoomView;
    
    public static event Action<GameObject> OnCardClicked;
    
    private void Start() {
        _cardZoomView = CardZoomView.Instance;
        _cardInfo = cardStats.cardInfo;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            _cardZoomView.ZoomCard(_cardInfo);
            return;
        }

        if(!cardStats.IsInteractable) return;

        OnCardClicked?.Invoke(gameObject);
    }
}
