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

    [Header("Game Stats")]
    public CardCollection cards;
    [SyncVar] public string playerName;
    [SyncVar, SerializeField] private int health;
    [SyncVar, SerializeField] private int score;
    public int Score { get => score; set => score = value; }

    public int Health
    {
        get => health;
        set
        {
            health = value;
            SetHealthValue(health);
        } 
    }
    
    public List<Phase> playerChosenPhases = new() {Phase.DrawI, Phase.DrawII};
    private List<GameObject> _discardSelection;
    public List<CardInfo> moneyCards;
    
    public static event Action OnCardPileChanged;
    public static event Action<PlayerManager, int> OnCashChanged;
    public static event Action<GameObject, bool> OnHandChanged;


    [Header("Turn Stats")]
    [SyncVar] private int _cash;
    public int Cash { 
        get => _cash;
        set{
            _cash = value;
            SetCashValue(_cash); // Invoke OnCashChanged and update UI
        }
    }
    
    [SyncVar] private int _recruits = 1;
    public int Recruits { 
        get => _recruits; 
        set{
            _recruits = value;
            SetRecruitValue(_recruits);
        }
    }
    
    [SyncVar] private int _deploys = 1;
    public int Deploys { 
        get => _deploys;
        set{
            _deploys = value;
            SetDeployValue(_deploys);
        }
    }

    public bool PlayerIsChoosingBlockers { get; private set; }
    private List<BattleZoneEntity> _blockers = new();
    
    public PlayerUI playerUI;
    public PlayerUI opponentUI;

    #region GameSetup
    public override void OnStartClient(){
        base.OnStartClient();

        cards = GetComponent<CardCollection>();
        // cards.deck.Callback += cards.OnDeckListChange;

        if (!isServer) return;
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        _combatManager = CombatManager.Instance;
    }

    [ClientRpc]
    public void RpcFindObjects(bool debug){   
        if(!hasAuthority) return;

        // var players = FindObjectsOfType<PlayerManager>();

        // if (debug) return;
        // opponent = players[0] == this ? players[1] : players[0];
    }

    [ClientRpc]
    public void RpcSetPlayerStats(int startHealth, int startScore){   
        playerName = isServer ? "Host" : "Client";
        var opponentName = !isServer ? "Host" : "Client"; // inverse of my name
        
        playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();

        if (hasAuthority){
            playerUI.SetPlayerUI(playerName, startHealth.ToString(), startScore.ToString());
        } else {
            opponentUI.SetOpponentUI(opponentName, startHealth.ToString(), startScore.ToString());
        }
    }
    
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
        card.GetComponent<CardMover>().MoveToDestination(hasAuthority, to);

        if (!hasAuthority) return;
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
        TurnManager.Instance.PlayerSelectedPhases(phases);
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> cardsToDiscard){
        _discardSelection = cardsToDiscard;
        TurnManager.Instance.PlayerSelectedDiscardCards();
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
        Cash++;
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
    
    private void SetHealthValue(int value){
        if (isServer) RpcSetHealthValue(value);
        else CmdSetHealthValue(value);
    }

    [Command]
    private void CmdSetHealthValue(int value){
        RpcSetHealthValue(value);
    }

    [ClientRpc]
    private void RpcSetHealthValue(int value){
        if(hasAuthority) playerUI.SetHealth(value);
        else opponentUI.SetHealth(value);
    }
        
    private void SetCashValue(int value){
        if (isServer) RpcSetCashValue(value);
        else CmdSetCashValue(value);
    }

    [Command]
    private void CmdSetCashValue(int value){
        RpcSetCashValue(value);
    }

    [ClientRpc]
    private void RpcSetCashValue(int value){
        OnCashChanged?.Invoke(this, value);
        
        if(hasAuthority) playerUI.SetCash(value);
        else opponentUI.SetCash(value);
    }
    
    private void SetDeployValue(int value){
        if (isServer) RpcSetDeployValue(value);
        else CmdSetDeployValue(value);
    }

    [Command]
    private void CmdSetDeployValue(int value){
        RpcSetDeployValue(value);
    }

    [ClientRpc]
    private void RpcSetDeployValue(int value){
        // OnDeployChanged?.Invoke(value);

        if(hasAuthority) playerUI.SetDeploys(value);
        else opponentUI.SetDeploys(value);
    }

    private void SetRecruitValue(int value){
        if (isServer) RpcSetRecruitValue(value);
        else CmdSetRecruitValue(value);
    }

    [Command]
    private void CmdSetRecruitValue(int value){
        RpcSetRecruitValue(value);
    }

    [ClientRpc]
    private void RpcSetRecruitValue(int value){
        // OnRecruitChanged?.Invoke(value);

        if(hasAuthority) playerUI.SetRecruits(value);
        else opponentUI.SetRecruits(value);
    }
    
    [ClientRpc] // ugh ds gieng sicher besser...
    public void RpcDestroyArrows()
    {
        if (!hasAuthority) return;
        
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

    public void PlayerPressedReadyButton()
    {
        if (isServer) _turnManager.PlayerPressedReadyButton(this);
        else CmdPlayerPressedReadyButton();
    }
    
    [Command]
    private void CmdPlayerPressedReadyButton() {
        _turnManager.PlayerPressedReadyButton(this);
    }

    #endregion
}