using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;
using System.Linq;
using Cysharp.Threading.Tasks;
using SorsGameState;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;
    private CardMover _cardMover;
    private UIManager _uiManager;

    [Header("Game State")]
    public List<PrevailOption> _chosenPrevailOptions = new();
    private List<CardStats> _selectedCards = new();
    private List<CardStats> _moneyCardsInPlay = new();

    public bool PlayerIsChoosingTarget { get; private set; }
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    private BattleZoneEntity _entity;
    public static event Action<PlayerManager, int> OnCashChanged;

    [Header("Card Collections")]
    public readonly CardCollection deck = new();
    public readonly CardCollection hand = new();
    public readonly CardCollection discard = new();

    #region Stats

    [Header("Game Stats")]
    [SyncVar, SerializeField] private string playerName;
    public string PlayerName
    {
        get => playerName;
        set => SetPlayerName(value);
    }

    private int _health;
    public int Health
    {
        get => _health;
        set => SetHealthValue(value);
    }

    private int _score;
    public int Score
    {
        get => _score;
        set => SetScoreValue(value);
    }

    [Header("Turn Stats")]
    private int _cash;
    public int Cash
    {
        get => _cash;
        set => SetMoneyValue(value); // Invoke OnCashChanged and update UI
    }

    [SyncVar] private int _buys = 1;
    public int Buys
    {
        get => _buys;
        set => SetBuyValue(value);
    }
    
    [SyncVar] private int _plays = 1;
    public int Plays
    {
        get => _plays;
        set => SetPlayValue(value);
    }

    [SyncVar] private int _prevails;
    public int Prevails
    {
        get => _prevails;
        set => SetPrevailValue(value);
    }

    public int ID { get; private set; }

    #endregion Stats

    public static event Action<BattleZoneEntity> OnPlayerChooseEntityTarget;

    #region GameSetup

    private void Awake()
    {
        CardPileClick.OnLookAtCollection += LookAtCollection;
    }

    [ClientRpc]
    public void RpcInitPlayer(int playerId)
    {
        _cardMover = CardMover.Instance;
        _uiManager = UIManager.Instance;

        EntityAndUISetup();

        ID = playerId;
        print("Player ID: " + ID);

        if (!isServer) return;
        _turnManager = TurnManager.Instance;
        _entity = GetComponent<BattleZoneEntity>();
    }

    private void EntityAndUISetup()
    {
        // TODO: Check if playerUI logic needs to be here or if it can be done in PlayerUI class
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
        
        var entity = GetComponent<BattleZoneEntity>();
        if(isOwned) {
            entity.SetPlayer(PlayerName, _playerUI);
            // Child 0 is the player stats BG
            _playerUI.SetEntity(entity, _playerUI.transform.GetChild(0).position);
        } else {
            entity.SetPlayer(PlayerName, _opponentUI);
            // Child 0 is the player stats BG
            _opponentUI.SetEntity(entity, _opponentUI.transform.GetChild(0).position);
        }
    }
    #endregion GameSetup

    #region Cards

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);
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
    public void DiscardSelection()
    {
        // TODO: Move this to turnManager ?, should include Other similar functions

        // Server calls each player object to discard their selection _selectedCards
        foreach (var card in _selectedCards)
        {
            RemoveHandCard(card);
            discard.Add(card);

            RpcMoveFromInteraction(card.gameObject, CardLocation.Hand, CardLocation.Discard);
        }
    }


    [Server]
    private void ShuffleDiscardIntoDeck()
    {
        var temp = new List<CardStats>();
        foreach (var card in discard)
        {
            temp.Add(card);
            deck.Add(card);

            // var cachedCard = GameManager.GetCardObject(card.cardInfo.goID);
            RpcMoveCard(card.gameObject, CardLocation.Discard, CardLocation.Deck);
        }

        foreach (var card in temp) discard.Remove(card);

        deck.Shuffle();
    }

    [ClientRpc]
    public void RpcMoveFromInteraction(GameObject card, CardLocation from, CardLocation to)
    {
        if(isOwned) from = CardLocation.Selection;
        _cardMover.MoveTo(card, isOwned, from, to);
    }

    [ClientRpc]
    public void RpcShowSpawnedCard(GameObject card, CardLocation destination) => _cardMover.ShowSpawnedCard(card, isOwned, destination).Forget();

    [ClientRpc]
    public void RpcShowSpawnedCards(List<GameObject> cards, CardLocation destination, bool fromFile) => _cardMover.ShowSpawnedCards(cards, isOwned, destination, fromFile).Forget();

    [Command]
    public void CmdPlayMoneyCard(CardStats card)
    {
        Cash += card.cardInfo.moneyValue;
        _moneyCardsInPlay.Add(card);

        RemoveHandCard(card);
        RpcPlayMoney(card.gameObject);
    }

    [Command]
    public void CmdUndoPlayMoney()
    {
        if (_moneyCardsInPlay.Count == 0 || Cash <= 0) return;
        ReturnMoneyToHand();
        _turnManager.PlayerClickedUndoButton(this);
    }

    [Server]
    public void ReturnMoneyToHand()
    {
        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new List<CardStats>();
        foreach (var card in _moneyCardsInPlay)
        {
            if (totalMoneyBack + card.cardInfo.moneyValue > Cash) continue;

            cardsToReturn.Add(card);
            totalMoneyBack += card.cardInfo.moneyValue;
        }

        // Return to hand
        foreach (var card in cardsToReturn)
        {
            _moneyCardsInPlay.Remove(card);
            Cash -= card.cardInfo.moneyValue;
            hand.Add(card);
            RpcReturnMoneyCardToHand(card.gameObject);
        }
    }

    [Server]
    public void DiscardMoneyCards()
    {
        if (_moneyCardsInPlay.Count == 0) return;

        foreach (var card in _moneyCardsInPlay)
        {
            discard.Add(card);
            RpcDiscardMoneyCard(card.gameObject);
        }

        _moneyCardsInPlay.Clear();
    }

    #endregion Cards

    #region Turn Actions

    [Command]
    public void CmdPhaseSelection(List<TurnState> phases)
    {
        print($"    - {PlayerName} selection: {string.Join(", ", phases)}");
        _turnManager.PlayerSelectedPhases(this, phases.ToArray());
    }

    [Command]
    public void CmdDiscardSelection(List<CardStats> cardsToDiscard)
    {
        _selectedCards = cardsToDiscard;
        _turnManager.PlayerSelectedDiscardCards(this, cardsToDiscard);
    }

    [Command]
    public void CmdConfirmBuy(MarketSelection card) => _turnManager.PlayerConfirmBuy(this, card);

    [Command]
    public void CmdConfirmPlay(CardStats card)
    {
        _turnManager.PlayerPlaysCard(this, card);
        RemoveHandCard(card);
    }

    [Command]
    public void CmdPrevailSelection(List<PrevailOption> options)
    {
        // Saving local player choice
        _chosenPrevailOptions = options;
        _turnManager.PlayerSelectedPrevailOptions(this, options);
    }

    [Command]
    public void CmdPrevailCardsSelection(List<CardStats> cards)
    {
        _selectedCards = cards;
        _turnManager.PlayerSelectedPrevailCards(this, cards);
    }

    [Command]
    public void CmdSkipInteraction() => _turnManager.PlayerSkipsInteraction(this);

    #endregion TurnActions

    #region Effect Interactions

    [TargetRpc]
    public void TargetPlayerStartChooseTarget()
    {
        // TODO: Need to expand this ?
        // Allows player to click entity and define target in targetArrowHandler (double fail save)
        // possibly must use for multiple targets
        PlayerIsChoosingTarget = true;
    }

    public void PlayerChoosesEntityTarget(BattleZoneEntity target)
    {
        if (isServer) OnPlayerChooseEntityTarget?.Invoke(target); // _abilityQueue.PlayerChoosesAbilityTarget(target);
        else CmdPlayerChoosesTargetEntity(target);

        PlayerIsChoosingTarget = false;
    }

    [Command]
    private void CmdPlayerChoosesTargetEntity(BattleZoneEntity target) => OnPlayerChooseEntityTarget?.Invoke(target); 

    #endregion

    #region Resources UI

    private void SetPlayerName(string name)
    {
        playerName = name;
        RpcUISetPlayerName(name);
    }

    [ClientRpc]
    public void RpcUISetPlayerName(string name)
    {
        playerName = name;
        if (isOwned) _playerUI.SetName(name);
        else _opponentUI.SetName(name);
    }

    [Server]
    private void SetHealthValue(int value)
    {
        _health = value;
        RpcUISetHealthValue(value);
    }

    [ClientRpc]
    private void RpcUISetHealthValue(int value)
    {
        if (isOwned) _playerUI.SetHealth(value);
        else _opponentUI.SetHealth(value);
    }

    [Server]
    private void SetScoreValue(int value)
    {
        _score = value;
        RpcUISetScoreValue(value);
    }

    [ClientRpc]
    private void RpcUISetScoreValue(int value)
    {
        if (isOwned) _playerUI.SetScore(value);
        else _opponentUI.SetScore(value);
    }

    [Server]
    private void SetMoneyValue(int value)
    {
        _cash = value;
        OnCashChanged?.Invoke(this, value);
        RpcUISetMoneyValue(value);
    }

    [ClientRpc]
    private void RpcUISetMoneyValue(int value)
    {
        if (isOwned) _playerUI.SetCash(value);
        else _opponentUI.SetCash(value);
    }

    [Server]
    private void SetBuyValue(int value)
    {
        _buys = value;
        RpcSetBuyValue(value);
    }

    [ClientRpc]
    private void RpcSetBuyValue(int value)
    {
        if (isOwned) _playerUI.SetBuys(value);
        else _opponentUI.SetBuys(value);
    }

    [Server]
    private void SetPlayValue(int value)
    {
        _plays = value;
        RpcSetPlayValue(value);
    }

    [ClientRpc]
    private void RpcSetPlayValue(int value)
    {
        if (isOwned) _playerUI.SetPlays(value);
        else _opponentUI.SetPlays(value);
    }

    [Server]
    private void SetPrevailValue(int value)
    {
        _prevails = value;
        RpcSetPrevailValue(value);
    }

    [ClientRpc]
    private void RpcSetPrevailValue(int value)
    {
        if (isOwned) _playerUI.SetPrevails(value);
        else _opponentUI.SetPrevails(value);
    }
    #endregion UI

    #region Utils

    public static PlayerManager GetLocalPlayer()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    // TODO: Does this make sense like this ? 
    // TODO: Could rework to not show what opponent is playing until confirmation
    [ClientRpc]
    private void RpcPlayMoney(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.Hand, CardLocation.MoneyZone);
    }

    [ClientRpc]
    private void RpcReturnMoneyCardToHand(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Hand);
    }

    [ClientRpc]
    private void RpcDiscardMoneyCard(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Discard);
    }

    [Server]
    private void RemoveHandCard(CardStats card)
    {
        var cardToRemove = hand.FirstOrDefault(c => c.Equals(card));
        hand.Remove(cardToRemove);
    }

    // public void PlayerPressedCombatButton() => CmdPlayerPressedCombatButton(this);
    // [Command]
    // public void CmdPlayerPressedCombatButton(PlayerManager player) => _combatManager.PlayerPressedReadyButton(player);

    private void LookAtCollection(CardLocation collectionType, bool ownsCollection)
    {
        // Only trigger from my own player object 
        if (! isOwned) return;
        
        CmdPlayerOpensCardCollection(this, collectionType, ownsCollection);
    }

    [Command]
    private void CmdPlayerOpensCardCollection(PlayerManager player, CardLocation collectionType, bool ownsCollection)
    {
        print($"Player {player.PlayerName} opens collection {collectionType}, owns collection {ownsCollection}");

        var collection = new CardCollection();
        if (collectionType == CardLocation.Discard)
            collection = ownsCollection ? player.discard : _turnManager.GetOpponentPlayer(player).discard;
        else if (collectionType == CardLocation.Trash)
            collection = _turnManager.GetTrashedCards();

        // TODO: 
        collection.OnUpdate += _uiManager.UpdateCardCollection;

        print("Collection count on server: " + collection.Count);
        _uiManager.TargetOpenCardCollection(player.connectionToClient, collection, collectionType, ownsCollection);
    }

    [ClientRpc]
    public void RpcSkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();
    [Server]
    internal BattleZoneEntity GetEntity() => _entity;

    [Server]
    public void ForceEndTurn() => _turnManager.ForceEndTurn();

    public bool Equals(PlayerManager other)
    {
        if (other == null) return false;

        // Optimization for a common success case.
        if (ReferenceEquals(this, other)) return true;

        return this.connectionToClient == other.connectionToClient;
    }

    private void OnDestroy()
    {
        CardPileClick.OnLookAtCollection += LookAtCollection;
    }

    #endregion
}