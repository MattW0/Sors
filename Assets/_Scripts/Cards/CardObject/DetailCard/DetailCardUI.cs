using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class DetailCardUI : CardUI, IPointerClickHandler
{
    public static event Action<CardInfo> OnInspect;

    internal void ShowDetailCard(CardInfo card)
    {
        SetCardUI(card, card.cardSpritePath);
        // TODO: Animate
        gameObject.SetActive(true);
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button != PointerEventData.InputButton.Right) return;

        OnInspect?.Invoke(CardInfo);
    }
}
