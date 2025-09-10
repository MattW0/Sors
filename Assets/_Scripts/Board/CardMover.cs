using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(CardSlotsManager))]
public class CardMover : MonoBehaviour
{
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
    private CardSlotsManager _slotManager;

    public static event Action OnUpdatePileNumbers;

    private void Awake() {
        ServiceLocator.Global.Register(this);
        _slotManager = GetComponent<CardSlotsManager>();
    }

    public void MoveTo(GameObject card, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var (sourcePile, destinationPile) = GetPiles(from, to, hasAuthority);
        MoveCard(hasAuthority, from, to, sourcePile, destinationPile, card);
    }

    public void MoveAllTo(List<GameObject> cards, bool hasAuthority, CardLocation from, CardLocation to)
    {
        var (sourcePile, destinationPile) = GetPiles(from, to, hasAuthority);
        // sourcePile.UpdatePosition = true;
        // var destinationPile = GetPile(to, hasAuthority);

        foreach(var card in cards) MoveCard(hasAuthority, from, to, sourcePile, destinationPile, card);
    }

    private void MoveCard(bool hasAuthority, CardLocation from, CardLocation to, CardsPileSors sourcePile, CardsPileSors destinationPile, GameObject card)
    {
        // Is front or back up ?
        FlipCard(card, hasAuthority, to);
        // ApplyScaling(card, from, to);
        ApplyMovement(sourcePile, destinationPile, card);
    }

    public async UniTaskVoid ShowSpawnedCard(GameObject card, bool hasAuthority, CardLocation destination)
    {
        InitSpawnedCard(card, hasAuthority);
        
        await UniTask.Delay(SorsTimings.showSpawnedCard);

        MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
    }

    public async UniTaskVoid ShowSpawnedCards(List<GameObject> cards, bool hasAuthority, CardLocation destination, bool fromFile)
    {
        foreach(var card in cards){
            InitSpawnedCard(card, hasAuthority, fromFile);
            await UniTask.Delay(SorsTimings.spawnCard);
        }

        await UniTask.Delay(SorsTimings.showSpawnedCard);

        foreach(var card in cards){
            MoveTo(card, hasAuthority, CardLocation.CardSpawn, destination);
            await UniTask.Delay(SorsTimings.spawnCard);
        }
    }

    private void InitSpawnedCard(GameObject card, bool hasAuthority=false, bool fromFile=false)
    {    
        // card.transform.localScale = Vector3.one;
        if(!fromFile) card.GetComponent<HandCardUI>().CardFrontUp();
        else {
            var destination = GetPile(CardLocation.CardSpawn, hasAuthority);
            FinishMove(destination, card);
        }

        card.SetActive(true);
    }

    #region Helpers
    private void ApplyMovement(CardsPileSors source, CardsPileSors destination, GameObject card)
    {
        if (source.pileType != CardLocation.CardSpawn)
            _slotManager.CardLeaves(source, card);

        card.transform.DOMove(destination.cardHolderTransform.position, SorsTimings.cardMoveTime)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => FinishMove(destination, card));
    }

    private void FinishMove(CardsPileSors pile, GameObject card)
    {
        _slotManager.CardArrives(pile, card).Forget();
        OnUpdatePileNumbers?.Invoke();
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
            || to == CardLocation.EntitySpawn)
        {
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
        // Only apply scaling for piles PlayZone, MoneyZone and Spawn
        // These have local scale 0.7 to reduce playboard space occupation        
        if(to == CardLocation.Hand)
            card.transform.DOScale(1.4f, SorsTimings.cardMoveTime);
        else if(from == CardLocation.Hand && (to == CardLocation.MoneyZone || to == CardLocation.PlayZone))
            card.transform.DOScale(0.7f, SorsTimings.cardMoveTime);
        else if (from == CardLocation.CardSpawn){
            card.transform.DOScale(0.5f, SorsTimings.cardMoveTime);
        } else if (to == CardLocation.EntitySpawn){
            card.transform.DOScale(3f, SorsTimings.cardMoveTime);
        } else if (from == CardLocation.EntitySpawn){
            card.transform.DOScale(0.25f, SorsTimings.cardMoveTime);
        }
    }

    public List<CardsPileSors> GetPiles() 
    {
        return new List<CardsPileSors> {
            playerHand,
            playerMoneyZone,
            playerPlayZone,
            playerDeck,
            playerDiscardPile,
            opponentHand,
            opponentMoneyZone,
            opponentPlayZone,
            opponentDeck,
            opponentDiscardPile,
            playerCardSpawn,
            opponentCardSpawn,
            selection,
            entitySpawn,
            trash
        };
    }

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
    Selection
}
