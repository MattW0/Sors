using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Mirror;
using Sirenix.OdinInspector;
using UnityEditor.UI;

[Serializable]
public class CardCollection : List<CardStats>
{

    // TODO: Make this a networkbehaviour class including CardCollectionManager and a synclist<CardStats>?
    // public readonly CardList Deck = new();
    // public readonly CardList Discard = new();
    // public readonly CardList Hand = new();
    public event Action<List<CardInfo>> OnUpdate;
    
    public void Shuffle()
    {
        var provider = new RNGCryptoServiceProvider();
        int n = Count;
        while (n > 1) {
            byte[] box = new byte[1];
            do provider.GetBytes (box);
            while (!(box[0] < n * (byte.MaxValue / n)));
            int k = box[0] % n;
            n--;
            (this[k], this[n]) = (this[n], this[k]);
        }
    }

    // public new void Add(CardStats card)
    // {
    //     base.Add(card);
    //     OnUpdate?.Invoke(ToCardInfos());
    //     // OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, card));
    // }

    public List<CardStats> ToCardStats()
    {
        var cardStats = new List<CardStats>();
        foreach (var card in this) cardStats.Add(card);
        return cardStats;
    }

    public List<CardInfo> ToCardInfos()
    {
        var cardInfos = new List<CardInfo>();
        foreach (var card in this) cardInfos.Add(card.cardInfo);
        return cardInfos;
    }

    public override string ToString()
    {
        return Count.ToString() + " cards";
    }
}