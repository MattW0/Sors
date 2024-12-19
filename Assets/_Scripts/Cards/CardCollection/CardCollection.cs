using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using Mirror;

public class CardCollection : NetworkBehaviour, ISerializationCallbackReceiver
{
    public readonly CardList deck = new();
    public readonly CardList discard = new();
    public readonly CardList hand = new();
    private readonly CardList _moneyCardsInPlay = new();
    private CardMover _cardMover;
    private PlayerManager _owner;

    // For serialization in unity inspector
    public string[] deckTitles;
    public string[] discardTitles;
    public string[] handTitles;

    private void Start()
    {
        _cardMover = CardMover.Instance;
        _owner = GetComponent<PlayerManager>();
    }

    [Server]
    public void DrawCards(int amount)
    {
        // First draw cards on Server, manipulating card collections
        amount = Math.Min(amount, discard.Count + deck.Count);

        List<GameObject> cards = new();
        for (var i = 0; i < amount; i++)
        {
            if (deck.Count == 0) ShuffleDiscardIntoDeck();

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            cards.Add(card.gameObject);
        }

        // The draw cards on Clients, draw animation
        ClientDrawing(cards).Forget();
    }

    [Server]
    public void RemoveHandCards(List<CardStats> cards, CardLocation destination)
    {
        foreach (var card in cards)
        {
            var cardToRemove = hand.FirstOrDefault(c => c.Equals(card));
            hand.Remove(cardToRemove);
        }

        if (destination == CardLocation.Discard) discard.AddRange(cards);
    }

    [Command]
    public void CmdPlayMoneyCard(CardStats card)
    {
        _owner.Cash += card.cardInfo.moneyValue;
        
        RemoveHandCards(new List<CardStats> { card }, CardLocation.MoneyZone);
        RpcMoveCard(card.gameObject, CardLocation.Hand, CardLocation.MoneyZone);

        _moneyCardsInPlay.Add(card);
    }

    [Command]
    public void CmdUndoPlayMoney()
    {
        if (_moneyCardsInPlay.Count == 0 || _owner.Cash <= 0) return;

        ReturnUnspentMoneyToHand();
    }

    [Server]
    public void DiscardMoneyCards()
    {
        if (_moneyCardsInPlay.Count == 0) return;

        // TODO: Does not give player the option to "discard" money cards
        // possibly remove if Undo works as intended
        ReturnUnspentMoneyToHand();

        foreach (var card in _moneyCardsInPlay)
        {
            discard.Add(card);
            RpcMoveCard(card.gameObject, CardLocation.MoneyZone, CardLocation.Discard);
        }

        _moneyCardsInPlay.Clear();
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);
    }

    [ClientRpc]
    public void RpcMoveFromInteraction(List<CardStats> cards, CardLocation from, CardLocation to)
    {
        if(isOwned) from = CardLocation.Selection;
        foreach(var c in cards) _cardMover.MoveTo(c.gameObject, isOwned, from, to);
    }

    [ClientRpc]
    public void RpcShowSpawnedCard(GameObject card, CardLocation destination) => _cardMover.ShowSpawnedCard(card, isOwned, destination).Forget();

    [ClientRpc]
    public void RpcShowSpawnedCards(List<GameObject> cards, CardLocation destination, bool fromFile) => _cardMover.ShowSpawnedCards(cards, isOwned, destination, fromFile).Forget();

    [Client]
    private async UniTaskVoid ClientDrawing(List<GameObject> cards)
    {
        // Opposing destination, moving the card objects with movement durations
        foreach(var card in cards)
        {
            RpcMoveCard(card, CardLocation.Deck, CardLocation.Hand);
            await UniTask.Delay(SorsTimings.draw);
        }
    }

    [Server]
    private void ReturnUnspentMoneyToHand()
    {
        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new List<CardStats>();
        foreach (var card in _moneyCardsInPlay)
        {
            if (totalMoneyBack + card.cardInfo.moneyValue > _owner.Cash) continue;

            cardsToReturn.Add(card);
            totalMoneyBack += card.cardInfo.moneyValue;
        }

        if (totalMoneyBack == 0) return;

        // Return to hand
        int undoAmount = 0;
        foreach (var card in cardsToReturn)
        {
            _moneyCardsInPlay.Remove(card);
            undoAmount += card.cardInfo.moneyValue;
            hand.Add(card);
            RpcMoveCard(card.gameObject, CardLocation.MoneyZone, CardLocation.Hand);
        }

        // Substract cash
        _owner.Cash -= undoAmount;
    }

    [Server]
    private void ShuffleDiscardIntoDeck()
    {
        var temp = new List<CardStats>();
        foreach (var card in discard)
        {
            temp.Add(card);
            deck.Add(card);
            
            RpcMoveCard(card.gameObject, CardLocation.Discard, CardLocation.Deck);
        }

        foreach (var card in temp) discard.Remove(card);

        deck.Shuffle();
    }

    public void OnBeforeSerialize()
    {
        deckTitles = deck.Select(c => c.cardInfo.title).ToArray();
        discardTitles = discard.Select(c => c.cardInfo.title).ToArray();
        handTitles = hand.Select(c => c.cardInfo.title).ToArray();
    }

    public void OnAfterDeserialize(){ }
}