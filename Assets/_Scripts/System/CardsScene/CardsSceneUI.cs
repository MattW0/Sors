using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CardsSceneUI : MonoBehaviour
{
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _deckButton;
    [SerializeField] private Button _creaturesButton;
    [SerializeField] private Button _technologiesButton;
    [SerializeField] private Button _moneyButton;

    private CardsSceneManager _cardsSceneManager;

    private void Start()
    {
        _cardsSceneManager = GetComponent<CardsSceneManager>();

        _backButton.onClick.AddListener(OnBack);
        _deckButton.onClick.AddListener(OnDeck);
        _creaturesButton.onClick.AddListener(OnCreatures);
        _technologiesButton.onClick.AddListener(OnTechnologies);
        _moneyButton.onClick.AddListener(OnMoney);
    }

    private void OnBack() => SceneManager.LoadScene("Lobby");
    private void OnDeck() => _cardsSceneManager.SelectCardType(CardType.Player);
    private void OnCreatures() => _cardsSceneManager.SelectCardType(CardType.Creature);
    private void OnTechnologies() => _cardsSceneManager.SelectCardType(CardType.Technology);
    private void OnMoney() => _cardsSceneManager.SelectCardType(CardType.Money);
}
