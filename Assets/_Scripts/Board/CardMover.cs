using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardMover : MonoBehaviour
{
    public static CardMover Instance { get; private set; }
    public static Dictionary<DetailCard, GameObject> Cache = new();

    [Header("Playboard Transforms")]
    [SerializeField] private CardsPileSors playerHand;
    [SerializeField] private CardsPileSors playerMoneyZone;
    [SerializeField] private CardsPileSors playerPlayZone;
    [SerializeField] private CardsPileSors playerDeck;
    [SerializeField] private CardsPileSors playerDiscardPile;
    [SerializeField] private CardsPileSors opponentHand;
    [SerializeField] private CardsPileSors opponentMoneyZone;
    [SerializeField] private CardsPileSors opponentPlayZone;
    [SerializeField] private CardsPileSors opponentDeck;
    [SerializeField] private CardsPileSors opponentDiscardPile;
    [SerializeField] private CardsPileSors trash;
    
    private void Awake(){
        if(!Instance) Instance = this;
    }

    public void Trash(GameObject card, bool b) => Instance.MoveTo(card, b, CardLocation.Hand, CardLocation.Trash);

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var cardUI = card.GetComponent<CardUI>();

        var sourcePile = GetPile(from, hasAuthority);
        if(sourcePile) sourcePile.Remove(card); // pile is null if card just spawned

        var destinationPile = GetPile(to, hasAuthority);
        destinationPile.Add(card);

        if(to == CardLocation.Discard || to == CardLocation.MoneyZone || to == CardLocation.Trash){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Hand && hasAuthority){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Deck) {
            cardUI.CardBackUp();
        }
        cardUI.HighlightReset();
    }

    private CardsPileSors GetPile(CardLocation location, bool hasAuthority){
        var pile = location switch{
            CardLocation.Spawned => null,
            CardLocation.Trash => trash,
            CardLocation.Deck => hasAuthority ? playerDeck : opponentDeck,
            CardLocation.Hand => hasAuthority ? playerHand : opponentHand,
            CardLocation.PlayZone => hasAuthority ? playerPlayZone : opponentPlayZone,
            CardLocation.MoneyZone => hasAuthority ? playerMoneyZone : opponentMoneyZone,
            CardLocation.Discard => hasAuthority ? playerDiscardPile : opponentDiscardPile,
            _ => null
        };

        return pile;
    }
    public void DiscardMoney(List<GameObject> cards, bool hasAuthority){
        foreach(var c in cards){
            MoveTo(c, false, CardLocation.MoneyZone, CardLocation.Discard);
            opponentHand.Remove(c);
        }
    }

    public void SpawnCard(GameObject card, bool hasAuthority, CardLocation destination){
        print("spawn card routine");
        print("card init pos: " + card.transform.position);
        
        var pile = GetPile(destination, hasAuthority);
        var pos = pile.transform.position;
        print("pile pos: " + pos);

        card.transform.DOMove(pos, 2f).OnComplete(() => {
            MoveTo(card, hasAuthority, CardLocation.Spawned, destination);
        });
    }
}

public enum CardLocation{
    Spawned,
    Trash,
    Deck,
    Hand,
    PlayZone,
    MoneyZone,
    Discard
}
