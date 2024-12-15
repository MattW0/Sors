using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Unity.VisualScripting;
using System.Collections.Specialized;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private EndScreen _endScreen;
    [SerializeField] private AlertDialogue _quitDialog;
    [SerializeField] private GameObject _cardCollectionViewPrefab;
    [SerializeField] private Transform _spawnParentTransform;

    // TODO: How to track open card collections?
    [SerializeField] private List<CardCollectionManager> _openCardCollections = new();
    public static SorsColors ColorPalette { get; private set; }

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
    public void TargetOpenCardCollection(NetworkConnection conn, List<CardStats> collection, CardLocation collectionType, bool isOwned)
    {
        print("Collection count on client: " + collection.Count);
        // TODO: Still show when empty?
        if (collection.Count == 0) return;

        var collectionManager = Instantiate(_cardCollectionViewPrefab, _spawnParentTransform).GetComponent<CardCollectionManager>();
        if (_openCardCollections.Contains(collectionManager)) return;

        collectionManager.OpenCardCollection(collection, collectionType, isOwned);
        _openCardCollections.Add(collectionManager);
    }

    public void UpdateCardCollection(List<CardInfo> cards)
    {
        RpcUpdateCardCollection(cards);
    }

    [ClientRpc]
    public void RpcUpdateCardCollection(List<CardInfo> cards)
    {
        print("Updating card collection");
    }

    private void CloseCardCollection(CardLocation collectionType)
    {
        
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
