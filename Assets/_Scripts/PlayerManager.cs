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
    public string PlayerName{
        get => playerName;
        set => SetPlayerName(value);
    }

    [Header("Entities")]
    private GameManager _gameManager;
    private TurnManager _turnManager;
    private CombatManager _combatManager;
    private BoardManager _boardManager;
    private CardCollectionPanel _cardCollectionPanel;
    private DropZoneManager _dropZone;
    private CardMover _cardMover;
    private Hand _handManager;
    public PlayerManager opponent { get; private set; }

    [Header("Game State")]
    public List<Phase> chosenPhases = new();
    public List<PrevailOption> chosenPrevailOptions = new();
    private Dictionary<CardLocation, List<GameObject>> _spawnedCards = new();
    private List<GameObject> _discardSelection = new();
    public List<GameObject> trashSelection = new();
    public Dictionary<GameObject, CardInfo> moneyCards = new();

    public bool PlayerIsChoosingBlockers { get; private set; }
    private List<CreatureEntity> _blockers = new();
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    
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
    public int Cash { 
        get => _cash;
        set => SetCashValue(value); // Invoke OnCashChanged and update UI
    }

    [SyncVar] private int _invents = 1;
    public int Invents { 
        get => _invents; 
        set => SetInventValue(value);
    }

    [SyncVar] private int _develops = 1;
    public int Develops { 
        get => _develops; 
        set => SetDevelopValue(value);
    }
    
    [SyncVar] private int _recruits = 1;
    public int Recruits { 
        get => _recruits; 
        set => SetRecruitValue(value);
    }
    
    [SyncVar] private int _deploys = 1;
    public int Deploys { 
        get => _deploys;
        set => SetDeployValue(value);
    }

    #region GameSetup

    public override void OnStartClient(){
        base.OnStartClient();

        if (!isServer) return;
        _gameManager = GameManager.Instance;
    }

    [ClientRpc]
    public void RpcInitPlayer(){
        _handManager = Hand.Instance;
        _cardMover = CardMover.Instance;
        _dropZone = DropZoneManager.Instance;
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
        
        if (!isServer) return;
        _turnManager = TurnManager.Instance;
        _combatManager = CombatManager.Instance;
        _boardManager = BoardManager.Instance;
        _cardCollectionPanel = CardCollectionPanel.Instance;
    }
    
    [Server] // GameManager calls this on player object
    public void DrawInitialHand(int amount)
    {
        for (var i = 0; i < amount; i++){
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
    public void RpcCardPilesChanged(){
        OnCardPileChanged?.Invoke();
    }
        
    [Server]
    public void DrawCards(int amount){
        while (amount > deck.Count + discard.Count){
            amount--;
        }

        for (var i = 0; i < amount; i++){
            if (deck.Count == 0) ShuffleDiscardIntoDeck();

            var cardInfo = deck[0];
            deck.RemoveAt(0);
            hand.Add(cardInfo);

            var cardObject = GameManager.GetCardObject(cardInfo.goID);
            RpcMoveCard(cardObject, CardLocation.Deck, CardLocation.Hand);
        }
    }

    [Server]
    private void ShuffleDiscardIntoDeck(){
        var temp = new List<CardInfo>();
        foreach (var card in discard){
            temp.Add(card);
            deck.Add(card);

            var cachedCard = GameManager.GetCardObject(card.goID);
            RpcMoveCard(cachedCard, CardLocation.Discard, CardLocation.Deck);
        }

        foreach (var card in temp){
            discard.Remove(card);
        }

        deck.Shuffle();
    }

    public void PlayCard(GameObject card) {
        if (isServer) RpcMoveCard(card, CardLocation.Hand, CardLocation.PlayZone);
        else CmdMoveCard(card, CardLocation.Hand, CardLocation.PlayZone);
    }

    [Command]
    private void CmdMoveCard(GameObject card, CardLocation from, CardLocation to){
        RpcMoveCard(card, from, to);
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocation from, CardLocation to){
        _cardMover.MoveTo(card, isOwned, from, to);

        if (!isOwned) return;
        if (to == CardLocation.Hand) _handManager.UpdateHandsCardList(card, true);
        else if (from == CardLocation.Hand) _handManager.UpdateHandsCardList(card, false);
    }

    [ClientRpc]
    public void RpcSpawnCard(GameObject card, CardLocation destination){
        card.SetActive(false);
        if(!_spawnedCards.ContainsKey(destination))
            _spawnedCards.Add(destination, new());
        _spawnedCards[destination].Add(card);
    }

    [ClientRpc]
    public void RpcResolveCardSpawn(bool animate){
        var waitTime = animate ? 0.5f : 0f;
        StartCoroutine(_cardMover.ResolveSpawn(_spawnedCards, isOwned, waitTime));
        _spawnedCards.Clear();
    }

    [Command]
    public void CmdPlayMoneyCard(GameObject card, CardInfo cardInfo){
        Cash += cardInfo.moneyValue;
        moneyCards.Add(card, cardInfo);

        TargetMoveMoneyCard(connectionToClient, card, false);
    }

    [Command]
    public void CmdUndoPlayMoney(){
        if (moneyCards.Count == 0) return;
        
        var totalMoneyPlayed = moneyCards.Sum(card => card.Value.moneyValue);
        if(totalMoneyPlayed > Cash) return; // Don't allow undo if player already spent money

        foreach(var (card, info) in moneyCards){
            Cash -= info.moneyValue;
            TargetMoveMoneyCard(connectionToClient, card, true);
        }
        moneyCards.Clear();
        _handManager.TargetHighlightMoney(connectionToClient);
    }

    [TargetRpc]
    private void TargetMoveMoneyCard(NetworkConnection conn, GameObject card, bool undo){
        if (undo) {
            _cardMover.MoveTo(card, isOwned, CardLocation.MoneyZone, CardLocation.Hand);
            _handManager.UpdateHandsCardList(card, true);
        } else {
            _cardMover.MoveTo(card, isOwned, CardLocation.Hand, CardLocation.MoneyZone);
            _handManager.UpdateHandsCardList(card, false);
        }
    }


    [Server]
    public void DiscardMoneyCards(){
        if (moneyCards.Count == 0) return;
        
        RpcDiscardMoneyCards(new List<GameObject>(moneyCards.Keys));
        foreach (var cardInfo in moneyCards.Values) {
            RemoveHandCard(cardInfo); // TODO: Check if this can be done on clients
            discard.Add(cardInfo);
        }
        
        moneyCards.Clear();
    }

    [ClientRpc]
    private void RpcDiscardMoneyCards(List<GameObject> cards){
        // Only need opponent list info 
        if(isOwned) return;
        _cardMover.DiscardMoney(cards, isOwned);
    }

    [ClientRpc]
    public void RpcTrashCard(GameObject card){
        _cardMover.Trash(card, isOwned);
    }

    #endregion Cards

    #region TurnActions

    [Command] 
    public void CmdPhaseSelection(List<Phase> phases){
        // Saving local player choice
        chosenPhases = phases;
        _turnManager.PlayerSelectedPhases(this, phases.ToArray());
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> cardsToDiscard){
        _discardSelection = cardsToDiscard;
        _turnManager.PlayerSelectedDiscardCards(this);
    }
    
    [Server]
    public void DiscardSelection(){
        // Server calls each player object to discard their selection _discardSelection
        foreach(var card in _discardSelection){
            var cardInfo = card.GetComponent<CardStats>().cardInfo;

            RemoveHandCard(cardInfo);
            discard.Add(cardInfo);
            RpcMoveCard(card, CardLocation.Hand, CardLocation.Discard);
        }

        _discardSelection.Clear();
    }

    public void PlayerSelectsKingdomTile(CardInfo card){
        if(isServer) _turnManager.PlayerSelectedKingdomTile(this, card);
        else CmdKingdomSelection(card);
    }

    [Command]
    public void CmdKingdomSelection(CardInfo card){
        _turnManager.PlayerSelectedKingdomTile(this, card);
    }

    [Command]
    public void CmdPlayCard(GameObject card){
        _turnManager.PlayerPlaysCard(this, card);
        RemoveHandCard(card.GetComponent<CardStats>().cardInfo);
        PlayCard(card);
    }

    [Command]
    public void CmdSkipCardPlay() => _turnManager.PlayerSkipsCardPlay(this);

    [Command]
    public void CmdPrevailSelection(List<PrevailOption> options){
        // Saving local player choice
        chosenPrevailOptions = options;
        _turnManager.PlayerSelectedPrevailOptions(this, options);
    }

    [Command]
    public void CmdTrashSelection(List<GameObject> cardsToTrash){
        trashSelection = cardsToTrash;
        _turnManager.PlayerSelectedTrashCards(this, cardsToTrash);
    }

    [Command]
    public void CmdSkipTrash() => _turnManager.PlayerSkipsTrash(this);

    #endregion TurnActions

    #region Combat

    public void PlayerChoosesBlocker(CreatureEntity blocker)
    {
        PlayerIsChoosingBlockers = true;
        _blockers.Add(blocker);
    }

    public void PlayerRemovesBlocker(CreatureEntity blocker)
    {
        _blockers.Remove(blocker);
        if (_blockers.Count == 0) PlayerIsChoosingBlockers = false;
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity target)
    {
        if (isServer) _combatManager.PlayerChoosesAttackerToBlock(target, _blockers);
        else CmdPlayerChoosesAttackerToBlock(target, _blockers);
        
        _blockers.Clear();
        PlayerIsChoosingBlockers = false;
    }

    [Command]
    private void CmdPlayerChoosesAttackerToBlock(CreatureEntity target, List<CreatureEntity> blockers)
    {
        _combatManager.PlayerChoosesAttackerToBlock(target, blockers);
    }

    #endregion

    #region UI
    private void SetPlayerName(string name){
        playerName = name;
        RpcUISetPlayerName(name);
    }

    [ClientRpc]
    public void RpcUISetPlayerName(string name){
        playerName = name;
        if(isOwned) _playerUI.SetName(name);
        else _opponentUI.SetName(name);
    }
    
    [Server]
    private void SetHealthValue(int value){
        _health = value;
        RpcUISetHealthValue(value);
    }

    [ClientRpc]
    private void RpcUISetHealthValue(int value){
        if(isOwned) _playerUI.SetHealth(value);
        else _opponentUI.SetHealth(value);
    }
    
    [Server]
    private void SetScoreValue(int value){
        _score = value;
        RpcUISetScoreValue(value);
    }

    [ClientRpc]
    private void RpcUISetScoreValue(int value){
        if(isOwned) _playerUI.SetScore(value);
        else _opponentUI.SetScore(value);
    }

    [Server]
    private void SetCashValue(int value){
        _cash = value;
        RpcUISetCashValue(value);
    }

    [ClientRpc]
    private void RpcUISetCashValue(int value){
        OnCashChanged?.Invoke(this, value);
        
        if(isOwned) _playerUI.SetCash(value);
        else _opponentUI.SetCash(value);
    }

    [Server]
    private void SetInventValue(int value){
        _invents = value;
        if(isServer) RpcUISetInventValue(value);
        else CmdUISetInventValue(value);
    }

    [Command]
    private void CmdUISetInventValue(int value) => RpcUISetInventValue(value);

    [ClientRpc]
    private void RpcUISetInventValue(int value){
        if(isOwned) _playerUI.SetInvents(value);
        else _opponentUI.SetInvents(value);
    }

    [Server]
    private void SetDevelopValue(int value){
        _develops = value;
        if (isServer) RpcUISetDevelopValue(value);
        else CmdUISetDevelopValue(value);
    }

    [Command]
    private void CmdUISetDevelopValue(int value) => RpcUISetDevelopValue(value);

    [ClientRpc]
    private void RpcUISetDevelopValue(int value){
        if(isOwned) _playerUI.SetDevelops(value);
        else _opponentUI.SetDevelops(value);
    }
    
    [Server]
    private void SetDeployValue(int value){
        _deploys = value;
        if (isServer) RpcSetDeployValue(value);
        else CmdSetDeployValue(value);
    }

    [Command]
    private void CmdSetDeployValue(int value) => RpcSetDeployValue(value);

    [ClientRpc]
    private void RpcSetDeployValue(int value){
        if(isOwned) _playerUI.SetDeploys(value);
        else _opponentUI.SetDeploys(value);
    }

    [Server]
    private void SetRecruitValue(int value){
        _recruits = value;
        if (isServer) RpcUISetRecruitValue(value);
        else CmdUISetRecruitValue(value);
    }

    [Command]
    private void CmdUISetRecruitValue(int value) => RpcUISetRecruitValue(value);

    [ClientRpc]
    private void RpcUISetRecruitValue(int value){
        if(isOwned) _playerUI.SetRecruits(value);
        else _opponentUI.SetRecruits(value);
    }
    
    [ClientRpc] // ugh ds gieng sicher besser...
    public void RpcDestroyArrows()
    {
        if (!isOwned) return;
        
        var arrows = FindObjectsOfType<ArrowRenderer>();
        foreach(var arrow in arrows) Destroy(arrow.gameObject);
    }
    
    #endregion UI

    #region Utils

    public static PlayerManager GetLocalPlayer()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    public void PlayerSkips() {
        // For Kingdom skip button in Develop, Recruit and Deploy
        if (isServer) PlayerSkips(this);
        else CmdPlayerSkips(this);
    }
    
    [Command]
    private void CmdPlayerSkips(PlayerManager player) => PlayerSkips(player);

    [Server]
    private void PlayerSkips(PlayerManager player) {
        _turnManager.PlayerIsReady(player);
    }

    private void RemoveHandCard(CardInfo cardInfo){
        var cardToRemove = hand.FirstOrDefault(c => c.Equals(cardInfo));
        hand.Remove(cardToRemove);
    }

    public void PlayerPressedCombatButton() {
        if (isServer) PlayerPressedCombatButton(this);
        else CmdPlayerPressedCombatButton(this);
    }

    [Command]
    private void CmdPlayerPressedCombatButton(PlayerManager player) => PlayerPressedCombatButton(player);

    [Server]
    private void PlayerPressedCombatButton(PlayerManager player) => _dropZone.PlayerPressedReadyButton(player);

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

    #endregion
}