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
    [SerializeField] private CardsPileSors playerCardSpawn;
    [SerializeField] private CardsPileSors opponentCardSpawn;
    [SerializeField] private CardsPileSors entitySpawn;
    [SerializeField] private CardsPileSors trash;
    
    private void Awake(){
        if(!Instance) Instance = this;
    }

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        // Remove from pile, cards positions are immediately updated in CardsPile
        var sourcePile = GetPile(from, hasAuthority);
        sourcePile.Remove(card);

        // Is front or back up ?
        FlipCard(card, hasAuthority, to);

        // Add to pile and only update position after movement is done
        var destinationPile = GetPile(to, hasAuthority);
        destinationPile.Add(card);

        ApplyScaling(card, from, to);
        ApplyMovement(destinationPile, card);
    }

    private void FlipCard(GameObject card, bool hasAuthority, CardLocation to){
        var cardUI = card.GetComponent<CardUI>();
        if(to == CardLocation.Discard 
            || to == CardLocation.MoneyZone 
            || to == CardLocation.Trash
            || to == CardLocation.EntitySpawn){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Hand && hasAuthority){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Hand && !hasAuthority){
            cardUI.CardBackUp();
        } else if (to == CardLocation.Deck) {
            cardUI.CardBackUp();
        }
    }

    private void ApplyScaling(GameObject card, CardLocation from, CardLocation to)
    {
        // Only apply scaling for piles PlayZone and MoneyZone
        // These have local scale 0.7 to reduce playboard space occupation        
        if(to == CardLocation.Hand)
            card.transform.DOScale(1.4f, 0.5f);
        else if(from == CardLocation.Hand || from == CardLocation.EntitySpawn || from == CardLocation.CardSpawn)
            if(to == CardLocation.MoneyZone || to == CardLocation.PlayZone)
                card.transform.DOScale(0.7f, 0.5f);
    }

    private void ApplyMovement(CardsPileSors pile, GameObject card)
    {
        var destinationTransform = pile.transform;

        card.transform.DOMove(destinationTransform.position, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() => {
            card.transform.SetParent(destinationTransform, true);
            card.transform.localScale = Vector3.one;
            pile.CardHasArrived(card);
        });
    }

    private CardsPileSors GetPile(CardLocation location, bool hasAuthority){
        var pile = location switch{
            CardLocation.CardSpawn => hasAuthority ? playerCardSpawn : opponentCardSpawn,
            CardLocation.EntitySpawn => entitySpawn,
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

    public IEnumerator ShowSpawnedCard(GameObject card, bool hasAuthority, CardLocation destination)
    {
        InitSpawnedCard(card, hasAuthority, destination);
        
        yield return new WaitForSeconds(1f);

        MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
    }

    public IEnumerator ShowSpawnedCards(List<GameObject> cards, bool hasAuthority, CardLocation destination)
    {
        foreach(var card in cards){
            InitSpawnedCard(card, hasAuthority, destination);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(2f);

        foreach(var card in cards){
            MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void InitSpawnedCard(GameObject card, bool hasAuthority, CardLocation destination)
    {    
        card.transform.localScale = Vector3.one;
        card.GetComponent<CardUI>().CardFrontUp();

        if(hasAuthority){
            playerCardSpawn.Add(card);
            playerCardSpawn.updatePosition = true;
        } else {
            opponentCardSpawn.Add(card);
            opponentCardSpawn.updatePosition = true;
        }
        card.SetActive(true);
    }
}

public enum CardLocation : byte
{
    CardSpawn,
    EntitySpawn,
    Trash,
    Deck,
    Hand,
    PlayZone,
    MoneyZone,
    Discard
}
