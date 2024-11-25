using System;
using UnityEngine;

public class DetailCardPreview : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private DetailCardUI _creatureDetailCard;
    [SerializeField] private DetailCardUI _technologyDetailCard;
    [SerializeField] private DetailCardUI _moneyDetailCard;
    [SerializeField] private EntityUI _creatureEntity;
    [SerializeField] private EntityUI _technologyEntity;

    [Header("Placement")]
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private Vector3 _cardHolderOffset;
    private Vector3 _cardHolderOriginalPosition;

    private void Awake()
    {
        _cardHolderOriginalPosition = _cardHolder.localPosition;
    }

    public void ShowPreview(CardInfo cardInfo, bool withEntity)
    {
        HideAll(withEntity);
        ShowCard(cardInfo);
        
        // Money card has no entity equivalent
        if(withEntity) ShowEntity(cardInfo);
    }

    private void ShowCard(CardInfo cardInfo)
    {
        var card = cardInfo.type switch{
            CardType.Creature => _creatureDetailCard,
            CardType.Technology => _technologyDetailCard,
            CardType.Money => _moneyDetailCard,
            _ => null
        };

        card.ShowDetailCard(cardInfo);
    }

    public void ShowEntity(CardInfo cardInfo)
    {
        _cardHolder.localPosition = _cardHolderOffset;

        var entity = cardInfo.type switch{
            CardType.Creature => _creatureEntity,
            CardType.Technology => _technologyEntity,
            _ => null
        };

        entity.InspectEntity(cardInfo);
    }

    public void HideAll(bool withEntity=false)
    {
        _cardHolder.localPosition = _cardHolderOriginalPosition;

        _creatureDetailCard.Hide();
        _technologyDetailCard.Hide();
        _moneyDetailCard.Hide();

        if(withEntity) {
            _creatureEntity.Hide();
            _technologyEntity.Hide();
        }
    }
}
