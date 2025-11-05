using UnityEngine.EventSystems;
using System;

public class DetailCardUI : CardUI, IPointerClickHandler
{
    public static event Action<CardInfo> OnInspect;

    internal void ShowDetailCard(CardInfo card)
    {
        SetCardUI(card, card.cardSpritePath);
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

    // TODO: Timed hover logic, maybe this is useful somewhere else sometime. Otherwise delete.
    //
    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (_traitsUI == null) return;

    //     // Use UniTask and a timer that will continue after 1s
    //     _isHovered = true;
    //     HoverCheckAsync().Forget();
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     if (_traitsUI == null) return;

    //     _isHovered = false;
    //     _traitsUI.ClearTraits();
    // }

    // private async UniTaskVoid HoverCheckAsync()
    // {
    //     // Don't start new timer if just hovering again
    //     if(_hoverDelayRunning) return;

    //     _hoverDelayRunning = true;
    //     await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());

    //     // still hovered after 1 second -> trigger detail
    //     if (_isHovered) _traitsUI?.InspectTraits();
    //     _hoverDelayRunning = false;
    // }
}
