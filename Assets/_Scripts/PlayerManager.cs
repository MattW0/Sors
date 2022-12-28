using System.Collections.Generic;
using UnityEngine;
using System;
using CardDecoder;
using Mirror;
using Unity.VisualScripting;

public class PlayerManager : NetworkBehaviour
{
    [Header("Entities")]
    private GameManager _gameManager;
    private TurnManager _turnManager;
    private CombatManager _combatManager;
    public PlayerManager opponent { get; private set; }

    [Header("Game State")]
    public List<Phase> playerChosenPhases = new() {Phase.DrawI, Phase.DrawII};
    private List<GameObject> _discardSelection;
    public List<CardInfo> moneyCards;
    
    public static event Action OnCardPileChanged;
    public static event Action<PlayerManager, int> OnCashChanged;
    public static event Action<GameObject, bool> OnHandChanged;

    [Header("Game Stats")]
    public CardCollection cards;
    [SyncVar, SerializeField] private string playerName;
    public string PlayerName{
        get => playerName;
        set => SetPlayerName(value);
    }

    [SyncVar, SerializeField] private int health;
    public int Health
    {
        get => health;
        set => SetHealthValue(value);
    }

    [SyncVar, SerializeField] private int score;
    public int Score 
    {
        get => score;
        set => SetScoreValue(value); 
    }

    [Header("Turn Stats")]
    [SyncVar] private int _cash;
    public int Cash { 
        get => _cash;
        set => SetCashValue(value); // Invoke OnCashChanged and update UI
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

    public bool PlayerIsChoosingBlockers { get; private set; }
    private List<BattleZoneEntity> _blockers = new();
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;

    #region GameSetup

    private void Awake(){
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
    }
    public override void OnStartClient(){
        base.OnStartClient();

        cards = GetComponent<CardCollection>();
        // cards.deck.Callback += cards.OnDeckListChange;

        if (!isServer) return;
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        _combatManager = CombatManager.Instance;
    }

    // [ClientRpc]
    // public void RpcSetPlayerStats(int startHealth, int startScore){
        
    //     PlayerName = PlayerName;
    //     if (!isOwned) return;

    //     _playerUI.SetPlayerUI(startHealth.ToString(), startScore.ToString());
    //     _opponentUI.SetPlayerUI(startHealth.ToString(), startScore.ToString());
    // }
    
    [Server] // GameManager calls this on player object
    public void DrawInitialHand(int amount)
    {
        for (var i = 0; i < amount; i++){
            var cardInfo = cards.deck[0];
            cards.deck.RemoveAt(0);
            cards.hand.Add(cardInfo);

            var cardObject = _gameManager.GetCardObject(cardInfo.goID);
            RpcMoveCard(cardObject, CardLocations.Deck, CardLocations.Hand);
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
        while (amount > cards.deck.Count + cards.discard.Count){
            amount--;
        } 

        for (var i = 0; i < amount; i++){
            if (cards.deck.Count == 0) ShuffleDiscardIntoDeck();

            var cardInfo = cards.deck[0];
            cards.deck.RemoveAt(0);
            cards.hand.Add(cardInfo);

            var cardObject = _gameManager.GetCardObject(cardInfo.goID);
            RpcMoveCard(cardObject, CardLocations.Deck, CardLocations.Hand);
        }
    }

    private void ShuffleDiscardIntoDeck(){
        print("Shuffling discard into deck");

        var temp = new List<CardInfo>();
        foreach (var card in cards.discard){
            temp.Add(card);
            cards.deck.Add(card);

            var cachedCard = _gameManager.GetCardObject(card.goID);
            RpcMoveCard(cachedCard, CardLocations.Discard, CardLocations.Deck);
        }

        foreach (var card in temp){
            cards.discard.Remove(card);
        }

        cards.deck.Shuffle();
    }

    public void PlayCard(GameObject card, bool isMoney=false) {
        
        var destination = CardLocations.PlayZone;
        if (isMoney) destination = CardLocations.MoneyZone;
        
        if (isServer) RpcMoveCard(card, CardLocations.Hand, destination);
        else CmdMoveCard(card, CardLocations.Hand, destination);
    }

    [Command]
    private void CmdMoveCard(GameObject card, CardLocations from, CardLocations to){
        RpcMoveCard(card, from, to);
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, CardLocations from, CardLocations to){
        card.GetComponent<CardMover>().MoveToDestination(isOwned, to);

        if (!isOwned) return;
        if (to == CardLocations.Hand) OnHandChanged?.Invoke(card, true);
        else if (from == CardLocations.Hand) OnHandChanged?.Invoke(card, false);
    }

    #endregion Cards

    #region TurnActions

    // !!! workarounds to communicate with server !!!

    [Command] 
    public void CmdPhaseSelection(List<Phase> phases){
        // Saving local player choice
        playerChosenPhases = phases;
        if (playerChosenPhases.Contains(Phase.Recruit)) Recruits++;
        _turnManager.PlayerSelectedPhases(this, phases.ToArray());
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> cardsToDiscard){
        _discardSelection = cardsToDiscard;
        _turnManager.PlayerSelectedDiscardCards(this);
    }
    
    [Server]
    public void DiscardSelection()
    {
        // Not an Rpc because Server handles _discardSelection and calls each player
        // object to discard their selection (in TurnManager.PlayerSelectedDiscardCards)
        
        foreach(var card in _discardSelection){
            var cardInfo = card.GetComponent<CardStats>().cardInfo;

            cards.hand.Remove(cardInfo);
            cards.discard.Add(cardInfo);

            RpcMoveCard(card, CardLocations.Hand, CardLocations.Discard);
        }
        _discardSelection.Clear();
    }

    [Command]
    public void CmdPlayMoneyCard(CardInfo cardInfo)
    {
        Cash += cardInfo.moneyValue;
        moneyCards.Add(cardInfo);
        cards.hand.Remove(cardInfo);
    }

    public void DiscardMoneyCards()
    {
        if (moneyCards.Count == 0) return;
        
        foreach (var card in moneyCards)
        {
            cards.discard.Add(card);
        }
        moneyCards.Clear();
    }

    [Command]
    public void CmdDeployCard(GameObject card, int holderNumber)
    {
        TurnManager.Instance.PlayerDeployedCard(this, card, holderNumber);
        PlayCard(card);
    }

    [TargetRpc]
    public void TargetDeployCard(NetworkConnection target)
    {
        // _myPlayZone.DeployToBattlezone();
    }

    [Command]
    public void CmdRecruitSelection(CardInfo card){
        
        Recruits--;
        if (card.title != null) Cash -= card.cost;
        
        TurnManager.Instance.PlayerSelectedRecruitCard(this, card);
    }

    #endregion TurnActions

    #region Combat

    public void PlayerChoosesBlocker(BattleZoneEntity blocker)
    {
        PlayerIsChoosingBlockers = true;
        _blockers.Add(blocker);
    }

    public void PlayerRemovesBlocker(BattleZoneEntity blocker)
    {
        _blockers.Remove(blocker);
        if (_blockers.Count == 0) PlayerIsChoosingBlockers = false;
    }

    public void PlayerChoosesAttackerToBlock(BattleZoneEntity target)
    {
        print("choosing to block with " + _blockers.Count);
        if (isServer) _combatManager.PlayerChoosesAttackerToBlock(target, _blockers);
        else CmdPlayerChoosesAttackerToBlock(target, _blockers);
        
        _blockers.Clear();
        PlayerIsChoosingBlockers = false;
    }

    [Command]
    private void CmdPlayerChoosesAttackerToBlock(BattleZoneEntity target, List<BattleZoneEntity> blockers)
    {
        _combatManager.PlayerChoosesAttackerToBlock(target, blockers);
    }

    #endregion

    #region UI
    private void SetPlayerName(string name){
        playerName = name;
        if (isServer) RpcUISetPlayerName(name);
        else CmdUISetPlayerName(name);
    }

    [Command]
    private void CmdUISetPlayerName(string name) => RpcUISetPlayerName(name);

    [ClientRpc]
    public void RpcUISetPlayerName(string name){
        playerName = name;
        if(isOwned) _playerUI.SetName(name);
        else _opponentUI.SetName(name);
    }
    
    private void SetHealthValue(int value){
        health = value;
        if (isServer) RpcUISetHealthValue(value);
        else CmdUISetHealthValue(value);
    }

    [Command]
    private void CmdUISetHealthValue(int value) => RpcUISetHealthValue(value);

    [ClientRpc]
    private void RpcUISetHealthValue(int value){
        if(isOwned) _playerUI.SetHealth(value);
        else _opponentUI.SetHealth(value);
    }
        
    private void SetScoreValue(int value){
        score = value;
        if (isServer) RpcUISetScoreValue(value);
        else CmdUISetScoreValue(value);
    }

    [Command]
    private void CmdUISetScoreValue(int value) => RpcUISetScoreValue(value);

    [ClientRpc]
    private void RpcUISetScoreValue(int value){
        if(isOwned) _playerUI.SetScore(value);
        else _opponentUI.SetScore(value);
    }
    private void SetCashValue(int value){

        _cash = value;
        if (isServer) RpcUISetCashValue(value);
        else CmdUISetCashValue(value);
    }

    [Command]
    private void CmdUISetCashValue(int value) => RpcUISetCashValue(value);

    [ClientRpc]
    private void RpcUISetCashValue(int value){
        OnCashChanged?.Invoke(this, value);
        
        if(isOwned) _playerUI.SetCash(value);
        else _opponentUI.SetCash(value);
    }
    
    private void SetDeployValue(int value){
        _deploys = value;
        if (isServer) RpcUISetDeployValue(value);
        else CmdUISetDeployValue(value);
    }

    [Command]
    private void CmdUISetDeployValue(int value) => RpcUISetDeployValue(value);

    [ClientRpc]
    private void RpcUISetDeployValue(int value){
        // OnDeployChanged?.Invoke(this, value);

        if(isOwned) _playerUI.SetDeploys(value);
        else _opponentUI.SetDeploys(value);
    }

    private void SetRecruitValue(int value){
        _recruits = value;
        if (isServer) RpcUISetRecruitValue(value);
        else CmdUISetRecruitValue(value);
    }

    [Command]
    private void CmdUISetRecruitValue(int value) => RpcUISetRecruitValue(value);

    [ClientRpc]
    private void RpcUISetRecruitValue(int value){
        // OnRecruitChanged?.Invoke(value);

        if(isOwned) _playerUI.SetRecruits(value);
        else _opponentUI.SetRecruits(value);
    }
    
    [ClientRpc] // ugh ds gieng sicher besser...
    public void RpcDestroyArrows()
    {
        if (!isOwned) return;
        
        var arrows = FindObjectsOfType<Arrow>();
        foreach(var arrow in arrows) Destroy(arrow.gameObject);
    }
    
    #endregion UI

    #region Utils

    public static PlayerManager GetPlayerManager()
    {
        var networkIdentity = NetworkClient.connection.identity;
        return networkIdentity.GetComponent<PlayerManager>();
    }

    public void PlayerPressedReadyButton() {

        if (isServer) _turnManager.PlayerPressedReadyButton(this);
        else CmdPlayerPressedReadyButton();
    }

    [Command]
    private void CmdPlayerPressedReadyButton() {
        _turnManager.PlayerPressedReadyButton(this);
    }

    #endregion
}