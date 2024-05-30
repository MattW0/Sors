using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PanelsManager : NetworkBehaviour
{
    // Singleton
    public static PanelsManager Instance { get; private set; }
    private TurnManager _turnManager;
    [SerializeField] private GameObject _cardCollectionViewPrefab;
    [SerializeField] private Transform _spawnParentTransform;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        _turnManager = TurnManager.Instance;
    }

    [TargetRpc]
    public void TargetOpenCardCollection(NetworkConnection conn, List<CardStats> cards, CardLocation collectionType, bool isOwned)
    {
        var cardInfos = new List<CardInfo>();
        foreach (var card in cards) cardInfos.Add(card.cardInfo);

        var cardCollection = Instantiate(_cardCollectionViewPrefab, _spawnParentTransform);
        cardCollection.GetComponent<CardSpawner>().SpawnDetailCardObjects(cardInfos);
        cardCollection.GetComponent<CardCollectionUI>().OpenCardCollection(collectionType, isOwned);
    }    
}
