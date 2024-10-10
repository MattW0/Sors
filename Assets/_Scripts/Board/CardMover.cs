using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private CardsPileSors playerCardSpawn;
    [SerializeField] private CardsPileSors opponentCardSpawn;
    [SerializeField] private CardsPileSors selection;
    [SerializeField] private CardsPileSors entitySpawn;
    [SerializeField] private CardsPileSors trash;
    [SerializeField] private CardsPileSors interaction;
    
    private void Awake()
    {
        if(!Instance) Instance = this;
    }

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var (sourcePile, destinationPile) = GetPiles(from, to, hasAuthority);

        // Is front or back up ?
        FlipCard(card, hasAuthority, to);

        // Update positions in CardsPileSors (remove updates immediately, add updates after movement is done)
        sourcePile.Remove(card);
        destinationPile.Add(card);

        // ApplyScaling(card, from, to);
        ApplyMovement(destinationPile, card);
    }

    public void MoveAllTo(List<GameObject> cards, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var (sourcePile, destinationPile) = GetPiles(from, to, hasAuthority);

        foreach(var card in cards){
            // Is front or back up ?
            FlipCard(card, hasAuthority, to);

            // Update positions in CardsPileSors (remove updates immediately, add updates after movement is done)
            sourcePile.Remove(card);
            destinationPile.Add(card);

            // ApplyScaling(card, from, to);
            ApplyMovement(destinationPile, card);
        }
    }
    
    public async UniTaskVoid ShowSpawnedCard(GameObject card, bool hasAuthority, CardLocation destination)
    {
        InitSpawnedCard(card, hasAuthority, destination);
        
        await UniTask.Delay(SorsTimings.showSpawnedCard);

        MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
    }

    public async UniTaskVoid ShowSpawnedCards(List<GameObject> cards, bool hasAuthority, CardLocation destination, bool fromFile)
    {
        foreach(var card in cards){
            InitSpawnedCard(card, hasAuthority, destination, fromFile);
            await UniTask.Delay(SorsTimings.spawnCard);
        }

        await UniTask.Delay(SorsTimings.showSpawnedCard);

        foreach(var card in cards){
            MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
            await UniTask.Delay(SorsTimings.moveSpawnedCard);
        }
    }

    private void InitSpawnedCard(GameObject card, bool hasAuthority, CardLocation destination, bool fromFile=false)
    {    
        card.transform.localScale = Vector3.one;
        if(!fromFile) card.GetComponent<HandCardUI>().CardFrontUp();

        if(hasAuthority){
            playerCardSpawn.Add(card);
            playerCardSpawn.UpdatePosition = true;
        } else {
            opponentCardSpawn.Add(card);
            opponentCardSpawn.UpdatePosition = true;
        }
        card.SetActive(true);
    }

    // TODO:
    
    // public void StartInteraction(CardLocation location, )
    // {

    //     GetPiles(location, CardLocation.Interaction, true);

    //     _cardHolder.DOMove(_transformInteractable.position, SorsTimings.cardPileRearrangement);
    //     _cardHolder.DOScale(_scaleInteractable, SorsTimings.cardPileRearrangement);
    // }

    // public void EndInteraction(CardLocation location)
    // {
    //     _cardHolder.DOMove(_transformDefault.position, SorsTimings.cardPileRearrangement);
    //     _cardHolder.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
    // }

    #region Helpers
    private void ApplyMovement(CardsPileSors pile, GameObject card)
    {
        var destinationTransform = pile.cardHolderTransform;

        card.transform.DOMove(destinationTransform.position, SorsTimings.cardMoveTime).SetEase(Ease.InOutCubic).OnComplete(() => {
            card.transform.SetParent(destinationTransform, true);
            // card.transform.localScale = Vector3.one;
            pile.CardHasArrived(card);
        });
    }

    private (CardsPileSors, CardsPileSors) GetPiles(CardLocation from, CardLocation to, bool hasAuthority)
    {
        // Change where card comes from because card moved on client already ( InteractionPanel.SelectCard() )
        if((to == CardLocation.EntitySpawn || to == CardLocation.Trash) && hasAuthority) 
            from = CardLocation.Selection;

        return (GetPile(from, hasAuthority), GetPile(to, hasAuthority));
    }

    private CardsPileSors GetPile(CardLocation location, bool hasAuthority)
    {
        var pile = location switch{
            CardLocation.CardSpawn => hasAuthority ? playerCardSpawn : opponentCardSpawn,
            CardLocation.Deck => hasAuthority ? playerDeck : opponentDeck,
            CardLocation.Hand => hasAuthority ? playerHand : opponentHand,
            CardLocation.PlayZone => hasAuthority ? playerPlayZone : opponentPlayZone,
            CardLocation.MoneyZone => hasAuthority ? playerMoneyZone : opponentMoneyZone,
            CardLocation.Discard => hasAuthority ? playerDiscardPile : opponentDiscardPile,
            CardLocation.EntitySpawn => entitySpawn,
            CardLocation.Trash => trash,
            CardLocation.Selection => selection,
            CardLocation.Interaction => interaction,
            _ => null
        };

        return pile;
    }

    private void FlipCard(GameObject card, bool hasAuthority, CardLocation to)
    {
        var cardUI = card.GetComponent<HandCardUI>();
        if(to == CardLocation.Discard 
            || to == CardLocation.MoneyZone 
            || to == CardLocation.Trash
            || to == CardLocation.EntitySpawn
            || to == CardLocation.Interaction){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Hand && hasAuthority){
            cardUI.CardFrontUp();
        } else if (to == CardLocation.Hand && !hasAuthority){
            cardUI.CardBackUp();
        } else if (to == CardLocation.Deck) {
            cardUI.CardBackUp();
        }
    }

    // private void ApplyScaling(GameObject card, CardLocation from, CardLocation to)
    // {
    //     // Only apply scaling for piles PlayZone, MoneyZone and Spawn
    //     // These have local scale 0.7 to reduce playboard space occupation        
    //     if(to == CardLocation.Hand)
    //         card.transform.DOScale(1.4f, SorsTimings.cardMoveTime);
    //     else if(from == CardLocation.Hand && (to == CardLocation.MoneyZone || to == CardLocation.PlayZone))
    //         card.transform.DOScale(0.7f, SorsTimings.cardMoveTime);
    //     else if (from == CardLocation.CardSpawn){
    //         card.transform.DOScale(0.5f, SorsTimings.cardMoveTime);
    //     } else if (to == CardLocation.EntitySpawn){
    //         card.transform.DOScale(3f, SorsTimings.cardMoveTime);
    //     } else if (from == CardLocation.EntitySpawn){
    //         card.transform.DOScale(0.25f, SorsTimings.cardMoveTime);
    //     }
    // }
    #endregion
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
    Discard,
    Selection,
    Interaction
}
