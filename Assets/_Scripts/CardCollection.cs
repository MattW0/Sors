using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Mirror;
using System;

[Serializable]
public struct CardInfo
{
    public string hash;
    public string title;
    public bool isCreature;
    public int cost;
    public int attack;
    public int health;
    public string goID;

    public CardInfo(ScriptableCard card, string _goID = null)
    {
        this.hash = card.hash;
        this.title = card.title;
        this.isCreature = card.isCreature;
        this.cost = card.cost;
        this.attack = card.attack;
        this.health = card.health;
        if (_goID != null) this.goID = _goID;
        else this.goID = null;
    }
}

public class CardCollection : NetworkBehaviour
{
    [Header("Player")]
    public PlayerManager player;

    [Header("CardCollections")]
    public readonly SyncListCard deck = new SyncListCard(); // Deck used during the match. Contains all cards in the deck. This is where we'll be drawing card froms.
    public readonly SyncListCard hand = new SyncListCard(); // Cards in player's hand during the match.
    public readonly SyncListCard graveyard = new SyncListCard(); // Cards in player graveyard.
}

public class SyncListCard : SyncList<CardInfo> {
    
    public void Shuffle(){
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider ();
        int n = Count;
        while (n > 1) {
            byte[] box = new byte[1];
            do provider.GetBytes (box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            CardInfo temp = this[k];
            this[k] = this[n];
            this[n] = temp;
        }
    }

    // Try and implement this?
    void OnCollectionUpdated(SyncListCard.Operation op, int index, CardInfo oldItem, CardInfo newItem)
    {
        switch (op)
        {
            case SyncListCard.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                break;
            case SyncListCard.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                break;
            case SyncListCard.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncListCard.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncListCard.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
}