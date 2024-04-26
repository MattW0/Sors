using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoomView : MonoBehaviour, IPointerClickHandler
{
    public static CardZoomView Instance { get; private set; }
    [SerializeField] private GameObject _cardZoomView;
    [SerializeField] private GameObject _cardHolder;
    [SerializeField] private Vector3 _cardHolderOffset;

    [Header("Prefabs")]
    [SerializeField] private GameObject _moneyDetailCard;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _creatureEntity;
    [SerializeField] private GameObject _technologyEntity;

    private GameObject _openedCardObject;
    private GameObject _openedEntityObject;

    private void Awake(){
        Instance = this;

        _cardZoomView.SetActive(false);
        _cardHolder.transform.localPosition = Vector3.zero;

        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
        _creatureEntity.SetActive(false);
        _technologyEntity.SetActive(false);
    }

    public void ZoomCard(CardInfo card){
        _cardZoomView.SetActive(true);

        // Set Card
        _openedCardObject = card.type switch{
            CardType.Creature => _creatureDetailCard,
            CardType.Technology => _technologyDetailCard,
            CardType.Money => _moneyDetailCard,
            _ => null
        };
        _openedCardObject.SetActive(true);
        var detailCard = _openedCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(card);
        detailCard.DisableFocus();

        // Money card has no entity equivalent
        if(card.type == CardType.Money) return;

        // Set entity
        _openedEntityObject = card.type switch{
            CardType.Creature => _creatureEntity,
            CardType.Technology => _technologyEntity,
            _ => null
        };

        _cardHolder.transform.localPosition = _cardHolderOffset;
        _openedEntityObject.SetActive(true);
        var entityUI = _openedEntityObject.GetComponent<EntityUI>();
        entityUI.SetEntityUI(card);
    }

    public void OnPointerClick(PointerEventData eventData){
        if (eventData.button == PointerEventData.InputButton.Left) Close();
    }

    public void Close(){
        _cardHolder.transform.localPosition = Vector3.zero;
        _cardZoomView.SetActive(false);
        _openedCardObject.SetActive(false);
        // is null if money card
        if(_openedEntityObject) _openedEntityObject.SetActive(false);
    }
}
