using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UIManager : NetworkBehaviour
{
    // Singleton
    public static UIManager Instance { get; private set; }
    // private TurnManager _turnManager;
    [SerializeField] private EndScreen _endScreen;
    [SerializeField] private GameObject _cardCollectionViewPrefab;
    [SerializeField] private Transform _spawnParentTransform;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        // _turnManager = TurnManager.Instance;
    }

    [TargetRpc]
    public void TargetOpenCardCollection(NetworkConnection conn, List<CardStats> cards, CardLocation collectionType, bool isOwned)
    {
        var cardInfos = new List<CardInfo>();
        foreach (var card in cards) cardInfos.Add(card.cardInfo);

        var cardCollection = Instantiate(_cardCollectionViewPrefab, _spawnParentTransform);
        cardCollection.GetComponent<CardSpawner>().SpawnDetailCardObjectsInGrid(cardInfos);
        cardCollection.GetComponent<CardCollectionUI>().OpenCardCollection(collectionType, isOwned);
    }

    [ClientRpc]
    public void RpcSetPlayerScore(PlayerManager player, int health, int score) => _endScreen.SetPlayerScore(player, health, score);

    [ClientRpc]
    public void RpcSetGameWinner(PlayerManager player) => _endScreen.SetGameWinner(player);

    [ClientRpc]
    internal void RpcSetDraw() => _endScreen.SetDraw();
}
