using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;
using System.Linq;
using Cysharp.Threading.Tasks;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;
    private CombatManager _combatManager;
    private AbilityQueue _abilityQueue;
    private CardMover _cardMover;
    private PhasePanelUI _phaseVisualsUI;
    private PlayerInterfaceManager _playerInterface;
    private PanelsManager _panelsManager;

    [Header("Game State")]
    public List<Phase> chosenPhases = new();
    public List<PrevailOption> chosenPrevailOptions = new();
    private List<GameObject> _selectedCards = new();
    public Dictionary<GameObject, CardStats> moneyCardsInPlay = new();

    public bool PlayerIsChoosingTarget { get; private set; }
    public bool PlayerIsChoosingAttack { get; private set; }
    public bool PlayerIsChoosingBlock { get; private set; }
    private List<CreatureEntity> _attackers = new();
    private List<CreatureEntity> _blockers = new();
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    private BattleZoneEntity _entity;
    public static event Action<PlayerManager, int> OnCashChanged;

    [Header("Card Collections")]
    public readonly CardCollection deck = new();
    public readonly CardCollection hand = new();
    public readonly CardCollection discard = new();

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

    #region GameSetup

    [ClientRpc]
    public void RpcInitPlayer()
    {
        _cardMover = CardMover.Instance;
        _phaseVisualsUI = PhasePanelUI.Instance;
        _panelsManager = PanelsManager.Instance;
        CardPileClick.OnLookAtCollection += LookAtCollection;

        EntityAndUISetup();

        if (!isServer) return;
        _playerInterface = PlayerInterfaceManager.Instance;
        _turnManager = TurnManager.Instance;
        _combatManager = CombatManager.Instance;
        _abilityQueue = AbilityQueue.Instance;
        _entity = GetComponent<BattleZoneEntity>();
    }

    private void EntityAndUISetup(){
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

            cards.Add(GameManager.GetCardObject(card.cardInfo.goID));
        }

        // The draw cards on Clients, moving the card objects with movement durations
        ClientDrawing(cards).Forget();
    }

    private async UniTaskVoid ClientDrawing(List<GameObject> cards)
    {
        foreach(var card in cards)
        {
            RpcMoveCard(card, CardLocation.Deck, CardLocation.Hand);
            await UniTask.Delay(SorsTimings.draw);
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

            var cachedCard = GameManager.GetCardObject(card.cardInfo.goID);
            RpcMoveCard(cachedCard, CardLocation.Discard, CardLocation.Deck);
        }

        foreach (var card in temp) discard.Remove(card);

        deck.Shuffle();
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);
    }

    [ClientRpc]
    public void RpcShowSpawnedCard(GameObject card, CardLocation destination) => _cardMover.ShowSpawnedCard(card, isOwned, destination).Forget();

    [ClientRpc]
    public void RpcShowSpawnedCards(List<GameObject> cards, CardLocation destination, bool fromFile) => _cardMover.ShowSpawnedCards(cards, isOwned, destination, fromFile).Forget();

    [Command]
    public void CmdPlayMoneyCard(GameObject cardObject, CardStats cardStats)
    {
        Cash += cardStats.cardInfo.moneyValue;
        moneyCardsInPlay.Add(cardObject, cardStats);

        RemoveHandCard(cardStats);
        RpcPlayMoney(cardObject);
    }

    [Command]
    public void CmdUndoPlayMoney()
    {
        if (moneyCardsInPlay.Count == 0 || Cash <= 0) return;
        ReturnMoneyToHand();
        _turnManager.PlayerClickedUndoButton(this);
    }

    [Server]
    public void ReturnMoneyToHand()
    {
        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new Dictionary<GameObject, CardStats>();
        foreach (var (card, stats) in moneyCardsInPlay)
        {
            if (totalMoneyBack + stats.cardInfo.moneyValue > Cash) continue;

            cardsToReturn.Add(card, stats);
            totalMoneyBack += stats.cardInfo.moneyValue;
        }

        // Return to hand
        foreach (var (card, stats) in cardsToReturn)
        {
            moneyCardsInPlay.Remove(card);
            Cash -= stats.cardInfo.moneyValue;
            hand.Add(stats);
            RpcReturnMoneyCardToHand(card);
        }
    }

    [Server]
    public void DiscardMoneyCards()
    {
        if (moneyCardsInPlay.Count == 0) return;

        foreach (var card in moneyCardsInPlay.Keys)
        {
            var cardInfo = moneyCardsInPlay[card];
            RpcDiscardMoneyCard(card);
            discard.Add(cardInfo);
        }

        moneyCardsInPlay.Clear();
    }

    #endregion Cards

    #region TurnActions

    [Command]
    public void CmdPhaseSelection(List<Phase> phases)
    {
        // Saving local player choice
        chosenPhases = phases;
        _turnManager.PlayerSelectedPhases(this, phases.ToArray());
    }

    [ClientRpc]
    public void RpcShowOpponentChoices(Phase[] phases)
    {
        if (isLocalPlayer) return;
        _phaseVisualsUI.ShowOpponentChoices(phases);
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> cardsToDiscard)
    {
        _selectedCards.Clear();
        _selectedCards = cardsToDiscard;
        _turnManager.PlayerSelectedDiscardCards(this);
    }

    [Server]
    public void DiscardSelection()
    {
        // Server calls each player object to discard their selection _selectedCards
        foreach (var card in _selectedCards)
        {
            var stats = card.GetComponent<CardStats>();

            RemoveHandCard(stats);
            discard.Add(stats);
            RpcMoveCard(card, CardLocation.Selection, CardLocation.Discard);

            _playerInterface.RpcLog($"{PlayerName} discards {stats.cardInfo.title}", LogType.Standard);
        }
    }

    [Command]
    public void CmdConfirmBuy(MarketSelection card) => _turnManager.PlayerConfirmBuy(this, card);

    [Command]
    public void CmdConfirmPlay(GameObject card)
    {
        _turnManager.PlayerPlaysCard(this, card);
        RemoveHandCard(card.GetComponent<CardStats>());
    }

    [Command]
    public void CmdPrevailSelection(List<PrevailOption> options)
    {
        // Saving local player choice
        chosenPrevailOptions = options;
        _turnManager.PlayerSelectedPrevailOptions(this, options);
    }

    [Command]
    public void CmdPrevailCardsSelection(List<GameObject> cards)
    {
        _selectedCards.Clear();
        _selectedCards = cards;
        _turnManager.PlayerSelectedPrevailCards(this, cards);
    }

    [Command]
    public void CmdSkipInteraction() => _turnManager.PlayerSkipsInteraction(this);

    #endregion TurnActions

    #region Combat

    public void PlayerChoosesAttacker(CreatureEntity attacker)
    {
        PlayerIsChoosingAttack = true;
        _attackers.Add(attacker);
    }

    public void PlayerRemovesAttacker(CreatureEntity attacker)
    {
        _attackers.Remove(attacker);
        if (_attackers.Count == 0) PlayerIsChoosingAttack = false;
    }

    public void PlayerChoosesTargetToAttack(BattleZoneEntity target)
    {
        CmdPlayerChoosesTargetToAttack(target, _attackers);
        _attackers.Clear();
        PlayerIsChoosingAttack = false;
    }

    [Command]
    private void CmdPlayerChoosesTargetToAttack(BattleZoneEntity target, List<CreatureEntity> attackers)
    {
        _combatManager.PlayerChoosesTargetToAttack(target, attackers);
    }

    public void PlayerChoosesBlocker(CreatureEntity blocker)
    {
        PlayerIsChoosingBlock = true;
        _blockers.Add(blocker);
    }

    public void PlayerRemovesBlocker(CreatureEntity blocker)
    {
        _blockers.Remove(blocker);
        if (_blockers.Count == 0) PlayerIsChoosingBlock = false;
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker)
    {
        CmdPlayerChoosesAttackerToBlock(attacker, _blockers);

        _blockers.Clear();
        PlayerIsChoosingBlock = false;
    }

    [Command]
    private void CmdPlayerChoosesAttackerToBlock(CreatureEntity target, List<CreatureEntity> blockers)
    {
        _combatManager.PlayerChoosesAttackerToBlock(target, blockers);
    }

    #endregion

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
        if (isServer) _abilityQueue.PlayerChoosesAbilityTarget(target);
        else CmdPlayerChoosesTargetEntity(target);

        PlayerIsChoosingTarget = false;
    }

    [Command]
    private void CmdPlayerChoosesTargetEntity(BattleZoneEntity target) => _abilityQueue.PlayerChoosesAbilityTarget(target);

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

    public void PlayerPressedCombatButton() => CmdPlayerPressedCombatButton(this);
    [Command]
    private void CmdPlayerPressedCombatButton(PlayerManager player) => _combatManager.PlayerPressedReadyButton(player);

    private void LookAtCollection(CardLocation collectionType, bool ownsCollection)
    {
        // Only trigger from my own player object 
        if (! isOwned) return;
        
        CmdPlayerOpensCardCollection(this, collectionType, ownsCollection);
    }

    [Command]
    private void CmdPlayerOpensCardCollection(PlayerManager player, CardLocation collectionType, bool ownsCollection)
    {
        print($"Player {player.PlayerName} opens collection {collectionType.ToString()}, owns collection {ownsCollection}");

        var cards = new List<CardStats>();
        if (collectionType == CardLocation.Discard){
            cards = player.discard;
            if(! ownsCollection) cards = _turnManager.GetOpponentPlayer(player).discard; 
        } else if (collectionType == CardLocation.Trash){
            cards = _turnManager.GetTrashedCards();
        }
        
        // TODO: Still show when empty?
        if (cards.Count == 0) return;
        _panelsManager.TargetOpenCardCollection(player.connectionToClient, cards, collectionType, ownsCollection);
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

    #endregion
}