using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class CardList : List<CardStats>
{
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

    public new void Add(CardStats card)
    {
        base.Add(card);
        OnUpdate?.Invoke(ToCardInfos());
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
