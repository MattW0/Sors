using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardStats cardStats;    
    public static event Action<CardInfo> OnInspect;
    public static event Action<GameObject> OnCardClicked;

    public void AddObserver(CardDragHandler dragHandler) {
        dragHandler.OnInspect += Inspect;
        dragHandler.OnSelect += Select;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) Inspect();
        else Select(true);
    }

    private void Select(bool b) {
        if(! cardStats.IsInteractable) return; 

        OnCardClicked?.Invoke(gameObject);
    }

    private void Inspect() => OnInspect?.Invoke(cardStats.cardInfo);
}
