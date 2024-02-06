using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;
using System.Linq;

public class PlayerManager : NetworkBehaviour
{
    public bool isAI;
    
    [Header("Entities")]
    private TurnManager _turnManager;
    private CombatManager _combatManager;
    private CardEffectsHandler _cardEffectsHandler;
    private CardMover _cardMover;
    private Hand _handManager;
    private PhasePanelUI _phaseVisualsUI;

    [Header("Game State")]
    public List<Phase> chosenPhases = new();
    public List<PrevailOption> chosenPrevailOptions = new();
    private List<GameObject> _selectedCards = new();
    public Dictionary<GameObject, CardInfo> moneyCardsInPlay = new();

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
    public readonly CardCollection money = new();

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

    // TODO: Why is this here? Overriding some BZE properties/functions ? Dafuq

    // [Server]
    // public void TakesDamage(int value, bool deathtouch){
    //     Health -= value;
    // }
    // public void Die() {}
    // public void RpcInitializeEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo) {}
    // public string Title { get; set; }
    // public CardType cardType { get; }

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

    #region GameSetup

    // public override void OnStartClient() => base.OnStartClient();

    [ClientRpc]
    public void RpcInitPlayer()
    {
        _handManager = Hand.Instance;
        _cardMover = CardMover.Instance;
        _phaseVisualsUI = PhasePanelUI.Instance;

        EntityAndUISetup();

        if (!isServer) return;
        _turnManager = TurnManager.Instance;
        _combatManager = CombatManager.Instance;
        _cardEffectsHandler = CardEffectsHandler.Instance;
    }

    private void EntityAndUISetup(){
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
        
        _entity = GetComponent<BattleZoneEntity>();
        if(isOwned) {
            _entity.SetPlayerUI(_playerUI);
            // Child 0 is the player stats BG
            _playerUI.SetEntity(_entity, _playerUI.transform.GetChild(0).position);
        } else {
            _entity.SetPlayerUI(_opponentUI);
            // Child 0 is the player stats BG
            _opponentUI.SetEntity(_entity, _opponentUI.transform.GetChild(0).position);
        }
    }

    [Server] // GameManager calls this on player object
    public void DrawInitialHand(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            var cardInfo = deck[0];
            deck.RemoveAt(0);
            hand.Add(cardInfo);

            var cardObject = GameManager.GetCardObject(cardInfo.goID);
            RpcMoveCard(cardObject, CardLocation.Deck, CardLocation.Hand);
        }
    }
    #endregion GameSetup

    #region GameEnd

    public void RpcGameIsDraw(int health)
    {

    }

    #endregion

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

            var cardInfo = deck[0];
            deck.RemoveAt(0);
            hand.Add(cardInfo);

            cards.Add(GameManager.GetCardObject(cardInfo.goID));
        }

        // The draw cards on Clients, moving the card objects with movement durations
        StartCoroutine(ClientDrawing(cards));
    }

    private IEnumerator ClientDrawing(List<GameObject> cards)
    {
        foreach(var card in cards){
            RpcMoveCard(card, CardLocation.Deck, CardLocation.Hand);
            yield return new WaitForSeconds(SorsTimings.draw);
        }
    }

    [Server]
    private void ShuffleDiscardIntoDeck()
    {
        var temp = new List<CardInfo>();
        foreach (var card in discard)
        {
            temp.Add(card);
            deck.Add(card);

            var cachedCard = GameManager.GetCardObject(card.goID);
            RpcMoveCard(cachedCard, CardLocation.Discard, CardLocation.Deck);
        }

        foreach (var card in temp) discard.Remove(card);

        deck.Shuffle();
    }

    [Command]
    public void CmdPlayCard(GameObject card)
    {
        _turnManager.PlayerPlaysCard(this, card);
        RemoveHandCard(card.GetComponent<CardStats>().cardInfo);
    }

    [Command]
    public void CmdSkipCardPlay() => _turnManager.PlayerSkipsCardPlay(this);    

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);

        if (!isOwned) return;
        if (to == CardLocation.Hand) _handManager.UpdateHandCardList(card, true);
        else if (from == CardLocation.Hand) _handManager.UpdateHandCardList(card, false);
    }

    [ClientRpc]
    public void RpcShowSpawnedCard(GameObject card, CardLocation destination) => StartCoroutine(_cardMover.ShowSpawnedCard(card, isOwned, destination));

    [ClientRpc]
    public void RpcShowSpawnedCards(List<GameObject> cards, CardLocation destination, bool fromFile) => StartCoroutine(_cardMover.ShowSpawnedCards(cards, isOwned, destination, fromFile));
    [TargetRpc]
    public void TargetSetHandCards(NetworkConnection conn, List<GameObject> handCards){
        var cardStats = new List<CardStats>();
        foreach(var c in handCards){
            cardStats.Add(c.GetComponent<CardStats>());
        }

        _handManager.SetHandCards(cardStats);  
    } 

    [Command]
    public void CmdPlayMoneyCard(GameObject card, CardInfo cardInfo)
    {
        Cash += cardInfo.moneyValue;
        moneyCardsInPlay.Add(card, cardInfo);

        // TargetMoveMoneyCard(connectionToClient, card, false, false);
        RemoveHandCard(cardInfo);
        RpcPlayMoney(card);
    }

    [Command]
    public void CmdUndoPlayMoney()
    {
        if (moneyCardsInPlay.Count == 0 || Cash <= 0) return;
        ReturnMoneyToHand(true);
    }

    [Server]
    public void ReturnMoneyToHand(bool isUndo)
    {
        // print($"ReturnMoneyToHand : {moneyCardsInPlay.Count} cards");

        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new Dictionary<GameObject, CardInfo>();
        foreach (var (card, info) in moneyCardsInPlay)
        {
            if (totalMoneyBack + info.moneyValue > Cash) continue;

            cardsToReturn.Add(card, info);
            totalMoneyBack += info.moneyValue;
        }

        // print($"CardsToReturn : {cardsToReturn.Count} cards");
        // Return to hand
        foreach (var (card, info) in cardsToReturn)
        {
            moneyCardsInPlay.Remove(card);
            Cash -= info.moneyValue;
            hand.Add(info);
            RpcReturnMoneyCardToHand(card);
        }

        if (isUndo) _handManager.TargetHighlightMoney(connectionToClient);
        else DiscardMoneyCards();
    }

    [Server]
    private void DiscardMoneyCards()
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
            var cardInfo = card.GetComponent<CardStats>().cardInfo;

            RemoveHandCard(cardInfo);
            discard.Add(cardInfo);
            RpcMoveCard(card, CardLocation.Hand, CardLocation.Discard);
        }
    }

    [Command]
    public void CmdSelectKingdomTile(CardInfo card, int cost) => _turnManager.PlayerSelectedKingdomTile(this, card, cost);

    [Command]
    public void CmdSkipKingdomBuy() => _turnManager.PlayerIsReady(this);

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
    public void CmdPlayerSkipsPrevailOption() => _turnManager.PlayerSkipsPrevailOption(this);

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
        if (isServer) _cardEffectsHandler.PlayerChoosesTargetEntity(target);
        else CmdPlayerChoosesTargetEntity(target);

        PlayerIsChoosingTarget = false;
    }

    [Command]
    private void CmdPlayerChoosesTargetEntity(BattleZoneEntity target) => _cardEffectsHandler.PlayerChoosesTargetEntity(target);

    #endregion


    #region UI

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
        RpcUISetMoneyValue(value);
    }

    [ClientRpc]
    private void RpcUISetMoneyValue(int value)
    {
        OnCashChanged?.Invoke(this, value);

        if (isOwned) _playerUI.SetCash(value);
        else _opponentUI.SetCash(value);
    }

    [Server]
    private void SetBuyValue(int value)
    {
        _buys = value;
        if (isServer) RpcSetBuyValue(value);
        else CmdUISetBuyValue(value);
    }

    [Command]
    private void CmdUISetBuyValue(int value) => RpcSetBuyValue(value);

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
        if (isServer) RpcSetPlayValue(value);
        else CmdUISetPlayValue(value);
    }

    [Command]
    private void CmdUISetPlayValue(int value) => RpcSetPlayValue(value);

    [ClientRpc]
    private void RpcSetPlayValue(int value)
    {
        if (isOwned) _playerUI.SetPlays(value);
        else _opponentUI.SetPlays(value);
    }

    #endregion UI

    #region Utils

    public static PlayerManager GetLocalPlayer()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    [ClientRpc]
    public void RpcUpdateHandCards(GameObject card, bool addingCard){
        _handManager.UpdateHandCardList(card, addingCard);
    }

    [ClientRpc]
    private void RpcPlayMoney(GameObject card)
    {
        // TODO: Does this make sense like this ? Bugness
        _cardMover.MoveTo(card, isOwned, CardLocation.Hand, CardLocation.MoneyZone);
        if (isOwned) _handManager.UpdateHandCardList(card, false);
    }

    [ClientRpc]
    private void RpcReturnMoneyCardToHand(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Hand);
        if (isOwned) _handManager.UpdateHandCardList(card, true);
    }

    [ClientRpc]
    private void RpcDiscardMoneyCard(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Discard);
    }

    [Server]
    private void RemoveHandCard(CardInfo cardInfo)
    {
        var cardToRemove = hand.FirstOrDefault(c => c.Equals(cardInfo));
        hand.Remove(cardToRemove);
    }

    public void PlayerPressedCombatButton()
    {
        if (isServer) PlayerPressedCombatButton(this);
        else CmdPlayerPressedCombatButton(this);
    }

    [Command]
    private void CmdPlayerPressedCombatButton(PlayerManager player) => PlayerPressedCombatButton(player);

    [Server]
    private void PlayerPressedCombatButton(PlayerManager player) => _combatManager.PlayerPressedReadyButton(player);

    // public void PlayerClickedCollectionViewButton() {
    //     if (isServer) PlayerClickedCollectionViewButton(this);
    //     else CmdPlayerClickedCollectionViewButton(this);
    // }

    // [Command]
    // private void CmdPlayerClickedCollectionViewButton(PlayerManager player) => PlayerClickedCollectionViewButton(player);

    // [Server]
    // private void PlayerClickedCollectionViewButton(PlayerManager player) {
    //     _cardCollectionPanel.TargetShowCardCollection(player.connectionToClient, hand);
    // }

    [ClientRpc]
    public void RpcSkipCardSpawnAnimations() => SorsTimings.SkipCardSpawnAnimations();

    [Server]
    public void ForceEndTurn() => _turnManager.ForceEndTurn();

    #endregion
}