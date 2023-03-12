using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardCollectionView : NetworkBehaviour
{
    public static CardCollectionView Instance { get; private set; }
    private List<DetailCard> _detailCards = new();
    [SerializeField] private GameObject _detailCardPrefab;
    [SerializeField] private Transform _grid;
    [SerializeField] private GameObject _view;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [TargetRpc]
    public void TargetShowCardCollection(NetworkConnection target, List<CardInfo> cards)
    {
        _detailCards.ForEach(card => Destroy(card.gameObject));
        _detailCards.Clear();
        foreach (var card in cards)
        {
            var cardObject = Instantiate(_detailCardPrefab) as GameObject;
            cardObject.transform.SetParent(_grid, false);

            var detailCard = cardObject.GetComponent<DetailCard>();
            detailCard.SetCardUI(card);
            _detailCards.Add(detailCard);
        }

        _view.SetActive(true);
    }

    public void CloseView()
    {
        _view.SetActive(false);
    }
}

public enum Views
{
    Hand,
    Deck,
    Discard,
    Trash,
    Supply,
    Other
}
