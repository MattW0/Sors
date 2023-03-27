using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class CardCollectionView : NetworkBehaviour
{
    public static CardCollectionView Instance { get; private set; }
    private List<DetailCard> _detailCards = new();
    private List<DetailCard> _chosenCards = new();
    [SerializeField] private GameObject _detailCardPrefab;
    [SerializeField] private Transform _gridAll;
    [SerializeField] private Transform _gridChosen;
    [SerializeField] private GameObject _container;
    [SerializeField] private TMP_Text _titleCollection;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [TargetRpc]
    public void TargetShowCardCollection(NetworkConnection target, List<CardInfo> cards, CollectionType type)
    {
        _detailCards.ForEach(card => Destroy(card.gameObject));
        _detailCards.Clear();

        _titleCollection.text = type.ToString();
        foreach (var card in cards)
        {
            var cardObject = Instantiate(_detailCardPrefab) as GameObject;
            cardObject.transform.SetParent(_gridAll, false);

            var detailCard = cardObject.GetComponent<DetailCard>();
            detailCard.SetCardUI(card);
            detailCard.isChoosable = true;
            _detailCards.Add(detailCard);
        }

        _container.SetActive(true);
    }

    public void AddCardToChosen(DetailCard card)
    {
        _chosenCards.Add(card);
        card.transform.SetParent(_gridChosen, false);
    }

    public void RemoveCardFromChosen(DetailCard card)
    {
        _chosenCards.Remove(card);
        card.transform.SetParent(_gridAll, false);
    }

    public void CloseView()
    {
        _container.SetActive(false);
    }
}

public enum CollectionType
{
    Hand,
    Deck,
    Discard,
    Trash,
    Supply,
    Other
}
