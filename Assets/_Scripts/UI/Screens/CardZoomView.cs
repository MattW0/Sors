using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZoomView : MonoBehaviour
{
    public static CardZoomView Instance { get; private set; }
    [SerializeField] private GameObject _cardZoomView;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _moneyDetailCard;
    private GameObject _openedCardObject;

    private void Awake(){
        Instance = this;

        _cardZoomView.SetActive(false);
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    public void ZoomCard(CardInfo card){
        _cardZoomView.SetActive(true);

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
    }

    public void OnClose(){
        _cardZoomView.SetActive(false);
        _openedCardObject.SetActive(false);
    }
}
