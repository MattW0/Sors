using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsSceneUI : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TMP_Dropdown _cardTypeSelection;
    [SerializeField] private Button _createCard;
    private CardsSceneManager _cardsSceneManager;

    private void Start()
    {
        _cardsSceneManager = GetComponent<CardsSceneManager>();

        // Player is a "free" type that represents starting Deck
        _createCard.onClick.AddListener(() => _cardsSceneManager.OpenCardCreateWindow());
        _cardTypeSelection.onValueChanged.AddListener((int i) => SelectType(i));
    }

    private void SelectType(int index)
    {
        CardType type = index switch
        {
            0 => CardType.All,
            1 => CardType.Creature,
            2 => CardType.Technology,
            3 => CardType.Money,
            4 => CardType.Player,
            _ => CardType.All
        };

        _cardsSceneManager.SelectCardType(type);
    }

    public void ScrollToTop() => _scrollRect.ScrollToTop();
}
