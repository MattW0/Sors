using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private EndScreen _endScreen;
    [SerializeField] private AlertDialogue _quitDialog;
    [SerializeField] private GameObject _cardCollectionViewPrefab;
    [SerializeField] private Transform _spawnParentTransform;
    [SerializeField] private List<CardLocation> _openCardCollections = new();
    public SorsColors ColorPalette { get; private set; }

    private void Awake()
    {
        if (!Instance) Instance = this;

        ColorPalette = Resources.Load<SorsColors>("Sors Colors");

        CardCollectionUI.OnCloseCardCollection += CloseCardCollection;
        PlayerInterfaceButtons.OnQuitButtonClicked += QuitDialog;
        
        AlertDialogue.OnAccept += AlertDialogueAccept;
        AlertDialogue.OnDecline += AlertDialogueDecline;
    }

    [TargetRpc]
    public void TargetOpenCardCollection(NetworkConnection conn, List<CardStats> cards, CardLocation collectionType, bool isOwned)
    {
        var cardInfos = new List<CardInfo>();
        foreach (var card in cards) cardInfos.Add(card.cardInfo);

        if (_openCardCollections.Contains(collectionType)) return;

        var cardCollection = Instantiate(_cardCollectionViewPrefab, _spawnParentTransform);
        cardCollection.GetComponent<CardSpawner>().SpawnDetailCardObjectsInGrid(cardInfos);
        cardCollection.GetComponent<CardCollectionUI>().OpenCardCollection(collectionType, isOwned);

        // TODO: Handle same type for player and opponent 
        _openCardCollections.Add(collectionType);
    }

    [ClientRpc]
    public void UpdateCardCollection(List<CardStats> cards, CardLocation collectionType)
    {
        // TODO:
    }

    private void CloseCardCollection(CardLocation collectionType)
    {
        _openCardCollections.Remove(collectionType);
    }

    [ClientRpc]
    public void RpcSetPlayerScore(PlayerManager player, int health, int score) => _endScreen.SetPlayerScore(player, health, score);

    [ClientRpc]
    public void RpcSetGameWinner(PlayerManager player) => _endScreen.SetGameWinner(player);

    [ClientRpc]
    internal void RpcSetDraw() => _endScreen.SetDraw();

    private void QuitDialog() => _quitDialog.WindowIn();

    private void AlertDialogueAccept(ModalWindowType type)
    {
        // TODO: Let other player win 
        Application.Quit();
    }

    private void AlertDialogueDecline(ModalWindowType type)
    {
        _quitDialog.WindowOut();
    }

    private void OnDestroy()
    {
        CardCollectionUI.OnCloseCardCollection -= CloseCardCollection;
        PlayerInterfaceButtons.OnQuitButtonClicked += QuitDialog;

        AlertDialogue.OnAccept -= AlertDialogueAccept;
        AlertDialogue.OnDecline -= AlertDialogueDecline;
    }
}
