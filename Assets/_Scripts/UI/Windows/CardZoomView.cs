using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoomView : ModalWindow, IPointerClickHandler
{
    

    [Header("Prefabs")]
    [SerializeField] private DetailCardPreview _cardPreview;

    private void Start()
    {
        _cardPreview.HideAll(true);

        CardClickHandler.OnInspect += InspectCardInfo;
        EntityClickHandler.OnInspect += InspectCardInfo;
        MarketTileUI.OnInspect += InspectCardInfo;
        DetailCardUI.OnInspect += InspectCardInfo;
    }

    public void InspectCardInfo(CardInfo card)
    {
        _cardPreview.ShowPreview(card, card.type != CardType.Money);
        WindowIn();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Close view on left click
        if (eventData.button != PointerEventData.InputButton.Left) return;

        WindowOut();

        _cardPreview.HideAll(true);
    }

    private void OnDestroy()
    {
        CardClickHandler.OnInspect -= InspectCardInfo;
        EntityClickHandler.OnInspect -= InspectCardInfo;
        MarketTileUI.OnInspect -= InspectCardInfo;
        DetailCardUI.OnInspect -= InspectCardInfo;
    }
}
