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
    [SerializeField] private CardPile playerHand;
    [SerializeField] private CardPile playerDeck;
    [SerializeField] private CardPile playerDiscardPile;
    [SerializeField] private CardPile trash;
    [SerializeField] private CardPile opponentHand;
    [SerializeField] private CardPile opponentDeck;
    [SerializeField] private CardPile opponentDiscardPile;
    [SerializeField] private CardPile playerPlayZone;
    [SerializeField] private CardPile opponentPlayZone;
    [SerializeField] private CardPile playerCardSpawn;
    [SerializeField] private CardPile opponentCardSpawn;
    [SerializeField] private CardPile entitySpawn;
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
        foreach(var card in cards) MoveCard(hasAuthority, from, to, sourcePile, destinationPile, card);
    }

    private void MoveCard(bool hasAuthority, CardLocation from, CardLocation to, CardPile sourcePile, CardPile destinationPile, GameObject card)
    {
        // Is front or back up ?
        FlipCard(card, hasAuthority, to);
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
        // print("CardMover: Init spawned card");
        if(!fromFile) card.GetComponent<HandCardUI>().CardFrontUp();

        var destination = GetPile(CardLocation.CardSpawn, hasAuthority);
        _slotManager.Initialize(destination, card);

        card.SetActive(true);
    }

    #region Helpers
    private void ApplyMovement(CardPile source, CardPile destination, GameObject card)
    {
        if (source.pileType != CardLocation.CardSpawn)
            _slotManager.CardLeaves(source, card);

        card.transform.DOMove(destination.CardPileTransformation.cardHolderTransform.position, SorsTimings.cardMoveTime)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => FinishMove(destination, card)
        );
    }

    private void FinishMove(CardPile pile, GameObject card)
    {
        _slotManager.CardArrives(pile, card);
        OnUpdatePileNumbers?.Invoke();
    }

    private (CardPile, CardPile) GetPiles(CardLocation from, CardLocation to, bool hasAuthority)
    {
        return (GetPile(from, hasAuthority), GetPile(to, hasAuthority));
    }

    private CardPile GetPile(CardLocation location, bool hasAuthority)
    {
        var pile = location switch{
            CardLocation.CardSpawn => hasAuthority ? playerCardSpawn : opponentCardSpawn,
            CardLocation.Deck => hasAuthority ? playerDeck : opponentDeck,
            CardLocation.Hand => hasAuthority ? playerHand : opponentHand,
            CardLocation.PlayZone => hasAuthority ? playerPlayZone : opponentPlayZone,
            CardLocation.Discard => hasAuthority ? playerDiscardPile : opponentDiscardPile,
            CardLocation.EntitySpawn => entitySpawn,
            CardLocation.Trash => trash,
            _ => null
        };

        return pile;
    }

    private void FlipCard(GameObject card, bool hasAuthority, CardLocation to)
    {
        var cardUI = card.GetComponent<HandCardUI>();
        if(to == CardLocation.Discard
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

    public List<CardPile> GetPiles() 
    {
        return new List<CardPile> {
            playerHand,
            playerPlayZone,
            playerDeck,
            playerDiscardPile,
            opponentHand,
            opponentPlayZone,
            opponentDeck,
            opponentDiscardPile,
            playerCardSpawn,
            opponentCardSpawn,
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
