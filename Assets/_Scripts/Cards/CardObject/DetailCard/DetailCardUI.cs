using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class DetailCardUI : CardUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler 
{
    [Header("Card UI")]
    public bool disableFocus = false;
    private Canvas _tempCanvas;
    private GraphicRaycaster _tempRaycaster;
    public static event Action<CardInfo> OnInspect;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button != PointerEventData.InputButton.Right) return;

        DefocusCard();
        OnInspect?.Invoke(CardInfo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(disableFocus) return;

        FocusCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DefocusCard();    
    }

    private void FocusCard()
    {
        // Creates a canvas on top of others
        _tempCanvas = gameObject.AddComponent<Canvas>();
        _tempCanvas.overrideSorting = true;
        _tempCanvas.sortingOrder = 1;
        _tempRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        
        // highlight.enabled = true;
        // highlight.DOColor(SorsColors.standardHighlight, 0.2f);
    }

    private void DefocusCard()
    {
        // Removes focus from the card
        Destroy(_tempRaycaster);
        Destroy(_tempCanvas);
        
        highlight.DOColor(SorsColors.transparent, 0.2f);
        highlight.enabled = false;
    }
    
    public void EnableHighlight() => highlight.enabled = true;
    public void DisableHighlight() => highlight.enabled = false;
}
