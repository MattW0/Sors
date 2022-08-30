using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Mirror;

public class CardCollection : NetworkBehaviour
{
    [Header("CardCollections")]
    public readonly SyncListCard deck = new SyncListCard();
    public readonly SyncListCard hand = new SyncListCard();
    public readonly SyncListCard discard = new SyncListCard(); 
}

public class SyncListCard : SyncList<CardInfo> {
    
    public void Shuffle(){
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider ();
        int n = Count;
        while (n > 1) {
            byte[] box = new byte[1];
            do provider.GetBytes (box);
            while (!(box[0] < n * (System.Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            CardInfo temp = this[k];
            this[k] = this[n];
            this[n] = temp;
        }
    }

    // // Try and implement this?
    // void OnCollectionUpdated(SyncListCard.Operation op, int index, CardInfo oldItem, CardInfo newItem)
    // {
    //     switch (op)
    //     {
    //         case SyncListCard.Operation.OP_ADD:
    //             Debug.Log("Adding card to collection");
    //             this[index] = newItem;
    //             // index is where it was added into the list
    //             // newItem is the new item
    //             break;
    //         case SyncListCard.Operation.OP_INSERT:
    //             // index is where it was inserted into the list
    //             // newItem is the new item
    //             break;
    //         case SyncListCard.Operation.OP_REMOVEAT:
    //             // index is where it was removed from the list
    //             // oldItem is the item that was removed
    //             break;
    //         case SyncListCard.Operation.OP_SET:
    //             // index is of the item that was changed
    //             // oldItem is the previous value for the item at the index
    //             // newItem is the new value for the item at the index
    //             break;
    //         case SyncListCard.Operation.OP_CLEAR:
    //             // list got cleared
    //             break;
    //     }
    // }
}