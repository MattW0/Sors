using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardStats cardStats;    
    public static event Action<CardInfo> OnInspect;
    public static event Action<GameObject> OnCardClicked;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right)
            OnInspect?.Invoke(cardStats.cardInfo);
        else {
            if(cardStats.IsInteractable) 
                OnCardClicked?.Invoke(gameObject);
        }
    }
}
