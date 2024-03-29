using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class CardCollection : List<CardInfo> {

    public List<CardInfo> _cards;

    // constructor
    public CardCollection(){
        _cards = new List<CardInfo>();
    }

    public void Shuffle(){
        var provider = new RNGCryptoServiceProvider();
        int n = Count;
        while (n > 1) {
            byte[] box = new byte[1];
            do provider.GetBytes (box);
            while (!(box[0] < n * (System.Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            (this[k], this[n]) = (this[n], this[k]);
        }
    }
}