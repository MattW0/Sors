using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoomView : ModalWindow, IPointerClickHandler
{
    [Header("Prefabs")]
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private Transform _traitDetailContainer;
    [SerializeField] private GameObject _traitDetailPrefab;

    private void Start()
    {
        _cardPreview.HideAll(true);

        CardDragHandler.OnInspect += InspectCardInfo;
        EntityClickHandler.OnInspect += InspectCardInfo;
        MarketTileUI.OnInspect += InspectCardInfo;
        DetailCardUI.OnInspect += InspectCardInfo;
    }

    public void InspectCardInfo(CardInfo card)
    {
        _cardPreview.ShowPreview(card, card.type != CardType.Money);
        StartTraitInspection(card.traits);

        WindowIn();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Close view on left click
        if (eventData.button != PointerEventData.InputButton.Left) return;

        WindowOut();

        _cardPreview.HideAll(true);
    }

    private void StartTraitInspection(List<Trait> traits)
    {
        foreach (Transform child in _traitDetailContainer) Destroy(child.gameObject);
        
        foreach (var trait in traits)
        {
            var traitDetail = Instantiate(_traitDetailPrefab, _traitDetailContainer);
            traitDetail.GetComponent<TraitDetailUI>().Initialize(trait);
        }
    }

    private void OnDestroy()
    {
        CardDragHandler.OnInspect -= InspectCardInfo;
        EntityClickHandler.OnInspect -= InspectCardInfo;
        MarketTileUI.OnInspect -= InspectCardInfo;
        DetailCardUI.OnInspect -= InspectCardInfo;
    }
}
