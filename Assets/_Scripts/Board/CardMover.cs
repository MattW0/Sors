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
    [SerializeField] private CardsPileSors spawned;
    [SerializeField] private CardsPileSors entitySpawn;
    
    private void Awake(){
        if(!Instance) Instance = this;
    }

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        // Remove from pile, cards positions are immediately updated in CardsPile
        var sourcePile = GetPile(from, hasAuthority);
        if(sourcePile) sourcePile.Remove(card); // pile is null if card just spawned
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
            CardLocation.CardSpawn => null,
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

    private IEnumerator SpawnCard(GameObject card, bool hasAuthority, CardLocation destination){
        
        // Add and reomove to correctly place card object in world space
        card.GetComponent<CardUI>().CardFrontUp();
        spawned.Add(card);
        spawned.Remove(card);
        card.SetActive(true);

        yield return new WaitForSeconds(0.7f);

        MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);

        yield return null;
    }

    public IEnumerator ResolveSpawn(Dictionary<CardLocation, List<GameObject>> cards, bool hasAuthority, float waitTime){
        foreach(var (dest, cardList) in cards){
            foreach(var card in cardList){
                StartCoroutine(SpawnCard(card, hasAuthority, dest));
                yield return new WaitForSeconds(waitTime);
            }
        }
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
