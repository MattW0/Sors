using System.Collections.Generic;
using UnityEngine;
using System;
using CardDecoder;
using Mirror;
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
    private CardMover _cardMover;
    private CardCollectionPanel _cardCollectionPanel;
    [SerializeField] private DropZoneManager _dropZone;
    public PlayerManager opponent { get; private set; }

    [Header("Game State")]
    public List<Phase> chosenPhases = new();
    public List<PrevailOption> chosenPrevailOptions = new();
    private List<GameObject> _discardSelection;
    public List<GameObject> trashSelection;
    public Dictionary<GameObject, CardInfo> moneyCards = new();

    public bool PlayerIsChoosingBlockers { get; private set; }
    private List<BattleZoneEntity> _blockers = new();
    private PlayerUI _playerUI;
    private PlayerUI _opponentUI;
    
    public static event Action OnCardPileChanged;
    public static event Action<PlayerManager, int> OnCashChanged;
    public static event Action<GameObject, bool> OnHandChanged;

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

    private void Start(){
        _dropZone = GameObject.Find("PlayerPlayZone").GetComponent<DropZoneManager>();
        _playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        _opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
    }

    public override void OnStartClient(){
        base.OnStartClient();

        if (!isServer) return;
        _gameManager = GameManager.Instance;
    }

    [ClientRpc]
    public void RpcInitPlayer(){
        _cardMover = CardMover.Instance;
        
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
        if (to == CardLocation.Hand) OnHandChanged?.Invoke(card, true);
        else if (from == CardLocation.Hand) OnHandChanged?.Invoke(card, false);
    }

    [Command]
    public void CmdPlayMoneyCard(GameObject card, CardInfo cardInfo){
        Cash += cardInfo.moneyValue;
        moneyCards.Add(card, cardInfo);
        hand.Remove(cardInfo);

        TargetMoveMoneyCard(connectionToClient, card);
    }

    [TargetRpc]
    private void TargetMoveMoneyCard(NetworkConnection conn, GameObject card){
        _cardMover.MoveTo(card, isOwned, CardLocation.Hand, CardLocation.MoneyZone);
        OnHandChanged?.Invoke(card, false);
    }

    [Server]
    public void DiscardMoneyCards(){
        if (moneyCards.Count == 0) return;
        
        RpcDiscardMoneyCards(new List<GameObject>(moneyCards.Keys));
        foreach (var cardInfo in moneyCards.Values) discard.Add(cardInfo);
        
        moneyCards.Clear();
    }

    [ClientRpc]
    private void RpcDiscardMoneyCards(List<GameObject> cards){

        if(isOwned) return;
        _cardMover.DiscardMoney(cards, isOwned);
    }

    [ClientRpc]
    public void RpcTrashCard(GameObject card){
        _cardMover.Trash(card, isOwned);
    }

    #endregion Cards

    #region TurnActions

    // !!! workarounds to communicate with server !!!

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

            hand.Remove(cardInfo);
            discard.Add(cardInfo);

            RpcMoveCard(card, CardLocation.Hand, CardLocation.Discard);
        }
        _discardSelection.Clear();
    }

    [Command]
    public void CmdUndoPlayMoney(){
        if (moneyCards.Count == 0) return;

        foreach(var (card, info) in moneyCards){
            // _gameManager.GetCardObject
            hand.Add(info);
            Cash -= info.moneyValue;
        }
        moneyCards.Clear();
    }

    public void MoneyReturned(List<GameObject> cards){
        foreach(var card in cards){
            if (isServer) RpcMoveCard(card, CardLocation.MoneyZone, CardLocation.Hand);
            else CmdMoveCard(card, CardLocation.Hand, CardLocation.Hand);
        }
    }

    // [Command]
    // public void CmdMoveMoneyCardsToDiscard(List<GameObject> cards) => RpcMoveMoneyCardsToDiscard(connectionToClient, cards);

    // [ClientRpc]
    // public void RpcMoveMoneyCardsToDiscard(NetworkConnection conn, List<GameObject> cards) => _cardMover.DiscardMoney(cards, isLocalPlayer);

    public void PlayerDevelops(List<CardInfo> selectedCards){
        if(isServer) _turnManager.PlayerSelectedDevelopCard(this, selectedCards);
        else CmdDevelopSelection(selectedCards);
    }

    [Command]
    public void CmdDevelopSelection(List<CardInfo> cards){
        _turnManager.PlayerSelectedDevelopCard(this, cards);
    }

    [Command]
    public void CmdDeployCard(GameObject card){
        _turnManager.PlayerDeployedCard(this, card);
        PlayCard(card);
    }

    [Command]
    public void CmdSkipDeploy() => _turnManager.PlayerSkipsDeploy(this);

    public void PlayerRecruits(CardInfo selectedCard){
        if(isServer) _turnManager.PlayerSelectedRecruitCard(this, selectedCard);
        else CmdRecruitSelection(selectedCard);
    }

    [Command]
    public void CmdRecruitSelection(CardInfo card){        
        _turnManager.PlayerSelectedRecruitCard(this, card);
    }

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
        switch (_turnManager.turnState)
        {
            case TurnState.Develop:
                _turnManager.PlayerIsReady(player);
                break;
            case TurnState.Recruit:
                _turnManager.PlayerIsReady(player);
                break;
            case TurnState.Deploy:
                _turnManager.PlayerIsReady(player);
                break;
        }
    }

    public void PlayerPressedReadyButton() {
        if (isServer) PlayerPressedReadyButton(this);
        else CmdPlayerPressedReadyButton(this);
    }

    [Command]
    private void CmdPlayerPressedReadyButton(PlayerManager player) => PlayerPressedReadyButton(player);

    [Server]
    private void PlayerPressedReadyButton(PlayerManager player) => _dropZone.PlayerPressedReadyButton(player);

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