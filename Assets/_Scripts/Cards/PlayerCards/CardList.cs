using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class CardList : List<CardStats>
{
    public CardListInfo Info;
    public CardList(bool isMine, CardLocation location) 
    {
        Info = new CardListInfo(isMine, location);
    }
    public event Action<CardListInfo, List<CardInfo>> OnUpdate;

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
        OnUpdate?.Invoke(Info, ToCardInfos());
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

public struct CardListInfo
{
    public bool isMine;
    public CardLocation location;
    public CardListInfo(bool isMine, CardLocation location)
    {
        this.isMine = isMine;
        this.location = location;
    }
}
