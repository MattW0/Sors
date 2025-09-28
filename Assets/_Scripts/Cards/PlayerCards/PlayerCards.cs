using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using Mirror;

public class PlayerCards : NetworkBehaviour, ISerializationCallbackReceiver
{
    public CardList deck;
    public CardList discard;
    public CardList hand;
    private List<CardStats> _clientMoneyCardsInPlay;
    private List<CardStats> _serverMoneyCardsToDiscard;

    // For serialization in unity inspector
    public string[] deckTitles;
    public string[] discardTitles;
    public string[] handTitles;
    public string[] moneyTitles;
    private CardMover _cardMover;
    private PlayerManager _owner;

    private void Start()
    {
        _cardMover = ServiceLocator.Global.Get<CardMover>();
        _owner = GetComponent<PlayerManager>();

        deck = new CardList(_owner.isLocalPlayer, CardLocation.Deck);
        discard = new CardList(_owner.isLocalPlayer, CardLocation.Discard);
        hand = new CardList(_owner.isLocalPlayer, CardLocation.Hand);
        _clientMoneyCardsInPlay = new CardList(_owner.isLocalPlayer, CardLocation.MoneyZone);

        _serverMoneyCardsToDiscard = new();
    }

    #region Server Logic

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

        RpcRemoveHandCards(cards, destination);

        if (destination == CardLocation.Discard) discard.AddRange(cards);
    }

    private void RpcRemoveHandCards(List<CardStats> cards, CardLocation destination)
    {
        _cardMover.MoveAllTo(cards.Select(c => c.gameObject).ToList(), isOwned, CardLocation.Hand, destination);
    }

    #endregion
    #region Client Logic

    [Client]
    public void PlayMoneyCard(CardStats card)
    {
        _clientMoneyCardsInPlay.Add(card);
        _owner.LocalCash += card.cardInfo.moneyValue;

        card.SetInteractable(false);
    }

    [Client] internal void ConfirmMoneyCards() => CmdConfirmMoneyCards(_clientMoneyCardsInPlay);

    [Command]
    private void CmdConfirmMoneyCards(List<CardStats> cards)
    {
        print($"{_owner.PlayerName} commits {cards.Count} money cards");
        _serverMoneyCardsToDiscard.AddRange(cards);
    }

    [Server]
    public void DiscardMoneyCards()
    {
        RemoveHandCards(_serverMoneyCardsToDiscard, CardLocation.Discard);
        RpcEndMoneyPlaying();
        _serverMoneyCardsToDiscard.Clear();
    }

    [ClientRpc]
    private void RpcEndMoneyPlaying()
    {
        _clientMoneyCardsInPlay.Clear();
    }

    [Client]
    public void UndoPlayMoney()
    {
        if (_clientMoneyCardsInPlay.Count == 0 || _owner.LocalCash <= 0) return;

        var temp = new List<CardStats>(_clientMoneyCardsInPlay);
        foreach (var card in temp)
        {
            _owner.LocalCash -= card.cardInfo.moneyValue;
            card.SetInteractable(true);
        }

        _clientMoneyCardsInPlay.Clear();
    }

    private void ReturnUnspentMoneyToHand()
    {
        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new List<CardStats>();
        foreach (var card in _clientMoneyCardsInPlay)
        {
            if (totalMoneyBack + card.cardInfo.moneyValue > _owner.LocalCash) continue;

            cardsToReturn.Add(card);
            totalMoneyBack += card.cardInfo.moneyValue;
        }

        if (totalMoneyBack == 0) return;

        // Return to hand
        int undoAmount = 0;
        foreach (var card in cardsToReturn)
        {
            _clientMoneyCardsInPlay.Remove(card);
            undoAmount += card.cardInfo.moneyValue;
            hand.Add(card);
            RpcMoveCard(card.gameObject, CardLocation.MoneyZone, CardLocation.Hand);
        }

        // Substract cash
        _owner.LocalCash -= undoAmount;
    }

    #endregion
    #region Helpers

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);
    }

    [ClientRpc]
    public void RpcMoveFromInteraction(List<CardStats> cards, CardLocation from, CardLocation to)
    {
        // if(isOwned) from = CardLocation.Selection;
        foreach(var c in cards) 
        {
            print($"Moving card {c.cardInfo.title} from {from}");
            _cardMover.MoveTo(c.gameObject, isOwned, from, to);
        }
    }

    [ClientRpc]
    public void RpcShowSpawnedCard(GameObject card, CardLocation destination)
    {
        _cardMover.ShowSpawnedCard(card, isOwned, destination).Forget();
        AddCardToCollection(card.GetComponent<CardStats>(), destination);
    }

    [ClientRpc]
    public void RpcShowSpawnedCards(List<GameObject> cards, CardLocation destination, bool fromFile)
    {
        _cardMover.ShowSpawnedCards(cards, isOwned, destination, fromFile).Forget();
        AddCardsToCollection(cards, destination);
    }


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

    private void AddCardsToCollection(List<GameObject> cards, CardLocation destination)
    {
        foreach (var card in cards) AddCardToCollection(card.GetComponent<CardStats>(), destination);
    }
    private void AddCardToCollection(CardStats card, CardLocation destination)
    {
        if (destination == CardLocation.Deck) _owner.Cards.deck.Add(card);
        else if(destination == CardLocation.Discard) _owner.Cards.discard.Add(card);
        else if(destination == CardLocation.Hand) _owner.Cards.hand.Add(card);
        else if(destination == CardLocation.PlayZone) { /* no-op because fringe case with state file loader */ }
        else Debug.LogWarning("Trying to add card to invalid location: " + destination);
    }

    #endregion

    public void OnBeforeSerialize()
    {
        if(deck == null || discard == null || hand == null) return;

        deckTitles = deck.Select(c => c.cardInfo.title).ToArray();
        discardTitles = discard.Select(c => c.cardInfo.title).ToArray();
        handTitles = hand.Select(c => c.cardInfo.title).ToArray();
    }

    public void OnAfterDeserialize(){ }
}