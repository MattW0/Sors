using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : MonoBehaviour
{
    public static CardMover Instance { get; private set; }

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
    
    private void Awake(){
        if(!Instance) Instance = this;
    }

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var cardUI = card.GetComponent<CardUI>();

        print("Moving card from " + from + " to " + to);

        var pile = GetPile(from, hasAuthority);
        if (pile == null) print("<color=red> Pile is null </color>");
        else pile.Remove(card);

        pile = GetPile(to, hasAuthority);
        pile.Add(card);

        switch (to){
        default:
            print("<color=orange> Unknown card destination </color>");
            break;
        case CardLocation.Deck:
            // if(hasAuthority) playerDeck.Add(card);
            // else opponentDeck.Add(card);

            cardUI.CardBackUp();
            break;
        case CardLocation.Hand:
            if (hasAuthority) {
                // trans.SetParent(playerHand, false);
                // playerHand.Add(card);
                cardUI.CardFrontUp();
                break;
            }
            // else opponentHand.Add(card);
            break;
        case CardLocation.PlayZone:
            // trans.SetParent(hasAuthority ? playerPlayZone : opponentPlayZone, false);
            break;
        case CardLocation.MoneyZone:
            // if (hasAuthority) playerMoneyZone.Add(card);
            // else {
            //     opponentMoneyZone.Add(card);
            cardUI.CardFrontUp();
            // }
            break;
        case CardLocation.Discard:
            // if (hasAuthority) playerDiscardPile.Add(card);
            // else opponentDiscardPile.Add(card);
            
            cardUI.CardFrontUp();
            // trans.localPosition = Vector3.zero;
            break;
        }

        cardUI.HighlightReset();
    }

    private CardsPileSors GetPile(CardLocation location, bool hasAuthority){
        var pile = location switch{
            CardLocation.Deck => hasAuthority ? playerDeck : opponentDeck,
            CardLocation.Hand => hasAuthority ? playerHand : opponentHand,
            CardLocation.PlayZone => hasAuthority ? playerPlayZone : opponentPlayZone,
            CardLocation.MoneyZone => hasAuthority ? playerMoneyZone : opponentMoneyZone,
            CardLocation.Discard => hasAuthority ? playerDiscardPile : opponentDiscardPile,
            _ => null
        };

        print("Pile: " + pile);
        return pile;
    }
}

public enum CardLocation{
    Spawned,
    Deck,
    Hand,
    PlayZone,
    MoneyZone,
    Discard
}
