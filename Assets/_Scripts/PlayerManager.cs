using System.Collections.Generic;
using UnityEngine;
using System;
using CardDecoder;
using Mirror;
using System.Linq;
using Unity.VisualScripting;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar, SerializeField] private string playerName;
    public string PlayerName
    {
        get => playerName;
        set => SetPlayerName(value);
    }

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
    private Dictionary<CardLocation, List<GameObject>> _spawnedCards = new();
    private List<GameObject> _selectedCards = new();
    public Dictionary<GameObject, CardInfo> moneyCards = new();

    public bool PlayerIsChoosingTarget { get; private set; }
    public bool PlayerIsChoosingAttack { get; private set; }
    public bool PlayerIsChoosingBlock { get; private set; }
    private List<CreatureEntity> _attackers = new();
    private List<CreatureEntity> _blockers = new();
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    private BattleZoneEntity _entity;

    public static event Action OnCardPileChanged;
    public static event Action<PlayerManager, int> OnCashChanged;

    // public CardCollection cards;

    [Header("CardCollections")]
    public readonly CardCollection deck = new();
    public readonly CardCollection hand = new();
    public readonly CardCollection discard = new();
    public readonly CardCollection money = new();

    [Header("Game Stats")]
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

    [SyncVar] private int _invents = 1;
    public int Invents
    {
        get => _invents;
        set => SetInventValue(value);
    }

    [SyncVar] private int _develops = 1;
    public int Develops
    {
        get => _develops;
        set => SetDevelopValue(value);
    }

    [SyncVar] private int _recruits = 1;
    public int Recruits
    {
        get => _recruits;
        set => SetRecruitValue(value);
    }

    [SyncVar] private int _deploys = 1;
    public int Deploys
    {
        get => _deploys;
        set => SetDeployValue(value);
    }

    #region GameSetup

    public override void OnStartClient() => base.OnStartClient();

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
            _playerUI.SetEntity(_entity);
            gameObject.transform.position = new Vector3(-9f, 0f, -4.75f);
            // gameObject.transform.localScale = Vector3.one;
        } else {
            _opponentUI.SetEntity(_entity);
            gameObject.transform.position = new Vector3(-9f, 0f, 4.75f);
            gameObject.transform.localScale = Vector3.one;
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

    [ClientRpc]
    public void RpcCardPilesChanged() => OnCardPileChanged?.Invoke();

    [Server]
    public void DrawCards(int amount)
    {
        while (amount > deck.Count + discard.Count)
        {
            amount--;
        }

        for (var i = 0; i < amount; i++)
        {
            if (deck.Count == 0) ShuffleDiscardIntoDeck();

            var cardInfo = deck[0];
            deck.RemoveAt(0);
            hand.Add(cardInfo);

            var cardObject = GameManager.GetCardObject(cardInfo.goID);
            RpcMoveCard(cardObject, CardLocation.Deck, CardLocation.Hand);
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

        foreach (var card in temp)
        {
            discard.Remove(card);
        }

        deck.Shuffle();
    }

    public void PlayCard(GameObject card)
    {
        if (isServer) RpcMoveCard(card, CardLocation.Hand, CardLocation.PlayZone);
        else CmdMoveCard(card, CardLocation.Hand, CardLocation.PlayZone);
    }

    [Command]
    private void CmdMoveCard(GameObject card, CardLocation from, CardLocation to) => RpcMoveCard(card, from, to);

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to)
    {
        _cardMover.MoveTo(card, isOwned, from, to);

        if (!isOwned) return;
        if (to == CardLocation.Hand) _handManager.UpdateHandsCardList(card, true);
        else if (from == CardLocation.Hand) _handManager.UpdateHandsCardList(card, false);
    }

    [ClientRpc]
    public void RpcSpawnCard(GameObject card, CardLocation destination)
    {
        card.SetActive(false);
        if (!_spawnedCards.ContainsKey(destination))
            _spawnedCards.Add(destination, new());
        _spawnedCards[destination].Add(card);
    }

    [ClientRpc]
    public void RpcResolveCardSpawn(bool animate)
    {
        var waitTime = animate ? 0.5f : 0f;
        StartCoroutine(_cardMover.ResolveSpawn(_spawnedCards, isOwned, waitTime));
        _spawnedCards.Clear();
    }

    [Command]
    public void CmdPlayMoneyCard(GameObject card, CardInfo cardInfo)
    {
        Cash += cardInfo.moneyValue;
        moneyCards.Add(card, cardInfo);

        // TargetMoveMoneyCard(connectionToClient, card, false, false);
        RemoveHandCard(cardInfo);
        RpcPlayMoney(card);
    }

    [Command]
    public void CmdUndoPlayMoney()
    {
        if (moneyCards.Count == 0 || Cash <= 0) return;
        ReturnMoneyToHand(true);
    }

    [Server]
    public void ReturnMoneyToHand(bool isUndo)
    {
        // Don't allow to return already spent money
        var totalMoneyBack = 0;
        var cardsToReturn = new Dictionary<GameObject, CardInfo>();
        foreach (var (card, info) in moneyCards)
        {
            if (totalMoneyBack + info.moneyValue > Cash) continue;

            cardsToReturn.Add(card, info);
            totalMoneyBack += info.moneyValue;
        }

        // Return to hand
        foreach (var (card, info) in cardsToReturn)
        {
            moneyCards.Remove(card);
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
        if (moneyCards.Count == 0) return;

        foreach (var card in moneyCards.Keys)
        {
            var cardInfo = moneyCards[card];
            RpcDiscardMoneyCard(card);
            discard.Add(cardInfo);
        }

        moneyCards.Clear();
    }


    [ClientRpc]
    public void RpcTrashCard(GameObject card) => _cardMover.Trash(card, isOwned);

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
    public void CmdPlayCard(GameObject card)
    {
        _turnManager.PlayerPlaysCard(this, card);
        RemoveHandCard(card.GetComponent<CardStats>().cardInfo);
        PlayCard(card);
    }

    [Command]
    public void CmdSkipCardPlay() => _turnManager.PlayerSkipsCardPlay(this);

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

    public void EntityTargetHighlight(bool targetable){
        if(isOwned) _playerUI.TargetHighlight(targetable, true);
        else _opponentUI.TargetHighlight(targetable, false);
    }

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
    private void SetInventValue(int value)
    {
        _invents = value;
        if (isServer) RpcUISetInventValue(value);
        else CmdUISetInventValue(value);
    }

    [Command]
    private void CmdUISetInventValue(int value) => RpcUISetInventValue(value);

    [ClientRpc]
    private void RpcUISetInventValue(int value)
    {
        if (isOwned) _playerUI.SetInvents(value);
        else _opponentUI.SetInvents(value);
    }

    [Server]
    private void SetDevelopValue(int value)
    {
        _develops = value;
        if (isServer) RpcUISetDevelopValue(value);
        else CmdUISetDevelopValue(value);
    }

    [Command]
    private void CmdUISetDevelopValue(int value) => RpcUISetDevelopValue(value);

    [ClientRpc]
    private void RpcUISetDevelopValue(int value)
    {
        if (isOwned) _playerUI.SetDevelops(value);
        else _opponentUI.SetDevelops(value);
    }

    [Server]
    private void SetDeployValue(int value)
    {
        _deploys = value;
        if (isServer) RpcSetDeployValue(value);
        else CmdSetDeployValue(value);
    }

    [Command]
    private void CmdSetDeployValue(int value) => RpcSetDeployValue(value);

    [ClientRpc]
    private void RpcSetDeployValue(int value)
    {
        if (isOwned) _playerUI.SetDeploys(value);
        else _opponentUI.SetDeploys(value);
    }

    [Server]
    private void SetRecruitValue(int value)
    {
        _recruits = value;
        if (isServer) RpcUISetRecruitValue(value);
        else CmdUISetRecruitValue(value);
    }

    [Command]
    private void CmdUISetRecruitValue(int value) => RpcUISetRecruitValue(value);

    [ClientRpc]
    private void RpcUISetRecruitValue(int value)
    {
        if (isOwned) _playerUI.SetRecruits(value);
        else _opponentUI.SetRecruits(value);
    }

    #endregion UI

    #region Utils

    public static PlayerManager GetLocalPlayer()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    [ClientRpc]
    private void RpcPlayMoney(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.Hand, CardLocation.MoneyZone);
        if (isOwned) _handManager.UpdateHandsCardList(card, false);
    }

    [ClientRpc]
    private void RpcReturnMoneyCardToHand(GameObject card)
    {
        _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Hand);
        if (isOwned) _handManager.UpdateHandsCardList(card, true);
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

    [Server]
    public void ForceEndTurn() => _turnManager.ForceEndTurn();

    #endregion
}