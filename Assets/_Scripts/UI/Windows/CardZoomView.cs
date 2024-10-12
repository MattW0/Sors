using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoomView : ModalWindow, IPointerClickHandler
{
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private Vector3 _cardHolderOffset;

    [Header("Prefabs")]
    [SerializeField] private GameObject _moneyDetailCard;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _creatureEntity;
    [SerializeField] private GameObject _technologyEntity;

    private GameObject _openedCardObject;
    private GameObject _openedEntityObject;

    private void Start()
    {
        _cardHolder.localPosition = Vector3.zero;

        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
        _creatureEntity.SetActive(false);
        _technologyEntity.SetActive(false);

        CardClickHandler.OnCardInspect += ZoomCard;
        EntityClickHandler.OnEntityInspect += ZoomCard;
        MarketTile.OnMarketTileInspect += ZoomCard;
    }

    public void ZoomCard(CardInfo card)
    {
        print("Zooming card: " + card.title);

        // Set Card
        _openedCardObject = card.type switch{
            CardType.Creature => _creatureDetailCard,
            CardType.Technology => _technologyDetailCard,
            CardType.Money => _moneyDetailCard,
            _ => null
        };
        
        _cardHolder.localPosition = Vector3.zero;
        _openedCardObject.SetActive(true);
        var detailCard = _openedCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(card);
        detailCard.DisableFocus();

        ModalWindowIn();

        // Money card has no entity equivalent
        if(card.type == CardType.Money) return;

        // Set entity
        _openedEntityObject = card.type switch{
            CardType.Creature => _creatureEntity,
            CardType.Technology => _technologyEntity,
            _ => null
        };

        _cardHolder.localPosition = _cardHolderOffset;
        _openedEntityObject.SetActive(true);
        var entityUI = _openedEntityObject.GetComponent<EntityUI>();
        entityUI.SetEntityUI(card);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Close view on left click
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ModalWindowOut();

        _openedCardObject.SetActive(false);
        // is null if money card
        if(_openedEntityObject) _openedEntityObject.SetActive(false);
    }

    private void OnDestroy()
    {
        CardClickHandler.OnCardInspect -= ZoomCard;
        EntityClickHandler.OnEntityInspect -= ZoomCard;
        MarketTile.OnMarketTileInspect -= ZoomCard;
    }
}
