using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Unity.VisualScripting;

public class PlayerManager : NetworkBehaviour
{
    [Header("Entities")]
    private GameManager _gameManager;
    private Kingdom _kingdom;
    public string playerName;
    public PlayerManager opponent;
    [SerializeField] private List<PlayZoneManager> _playZones;


    [Header("Game Stats")]
    public CardCollection cards;
    [SyncVar, SerializeField] private int _health;
    [SyncVar, SerializeField] private int _score;
    public int Health { get => _health; set => _health = value; }
    public int Score { get => _score; set => _score = value; }

    public List<Phase> playerChosenPhases = new List<Phase>() {Phase.DrawI, Phase.DrawII};
    private List<GameObject> _discardSelection = new List<GameObject>();
    
    public static event Action OnCardPileChanged;
    public static event Action<int> OnCashChanged;

    public List<GameObject> moneyCards;

    [Header("Turn Stats")]
    [SyncVar, SerializeField] private int cash;
    public int Cash { 
        get => cash;
        set{
            cash = value;
            SetCashValue(cash);
            OnCashChanged?.Invoke(cash);
        }
    }
    [SyncVar] private int _recruits = 1;
    public int Recruits { 
        get => _recruits; 
        set{
            _recruits = value;
            SetRecruitValue(_recruits);
            OnCashChanged?.Invoke(Cash);
        }
    }

    public PlayerUI playerUI;
    public PlayerUI opponentUI;

    #region GameSetup
    public override void OnStartClient(){
        base.OnStartClient();

        cards = GetComponent<CardCollection>();
        if (isServer) _gameManager = GameManager.Instance;
    }

    [ClientRpc]
    public void RpcFindObjects(bool debug){   
        if(!hasAuthority) return;

        var players = FindObjectsOfType<PlayerManager>();
        if(!debug) opponent = players[0] == this ? players[1] : players[0];
        
        _kingdom = Kingdom.Instance;
        _playZones.AddRange(FindObjectsOfType<PlayZoneManager>());
    }

    [ClientRpc]
    public void RpcSetPlayerStats(int startHealth, int startScore){   
        playerName = isServer ? "Host" : "Client";
        var opponentName = !isServer ? "Host" : "Client"; // inverse of my name
        
        playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();

        if (hasAuthority){
            _health = startHealth;
            _score = startScore;
            playerUI.SetPlayerUI(playerName, startHealth.ToString(), startScore.ToString());
        } else {
            opponentUI.SetOpponentUI(opponentName, startHealth.ToString(), startScore.ToString());
        }
    }

    #endregion GameSetup

    #region Cards

    [ClientRpc]
    public void RpcCardPilesChanged(){
        OnCardPileChanged?.Invoke();
    }

    public void DrawCards(int amount){

        if(!isServer) return;

        while (amount > cards.deck.Count + cards.discard.Count){
            amount--;
        } 

        for (int i = 0; i < amount; i++){
            if (cards.deck.Count == 0) ShuffleDiscardIntoDeck();

            CardInfo card = cards.deck[0];
            cards.deck.RemoveAt(0);
            cards.hand.Add(card);

            GameObject cardObject = _gameManager.GetCardObject(card.goID);
            RpcMoveCard(cardObject, CardLocations.Deck, CardLocations.Hand);
        }
    }

    private void ShuffleDiscardIntoDeck(){
        print("Shuffling discard into deck");

        var temp = new List<CardInfo>();
        foreach (var card in cards.discard){
            temp.Add(card);
            cards.deck.Add(card);

            GameObject cachedCard = _gameManager.GetCardObject(card.goID);
            RpcMoveCard(cachedCard, CardLocations.Discard, CardLocations.Deck);

        }

        foreach (var card in temp){
            cards.discard.Remove(card);
        }

        cards.deck.Shuffle();
    }

    [TargetRpc]
    public void TargetDiscardCards(NetworkConnection target, int nbToDiscard){
        if (cards.hand.Count == 0) return;

        // foreach (CardUI _ui in playerHand.GetComponentsInChildren<CardUI>()){
        //     _ui.Highlight(true);
        // }
    }

    // public void CardPileNumberChanged() => OnCardPileChanged?.Invoke();

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
        // if(from == CardLocations.Hand) cards.hand.Remove(card.GetComponent<CardStats>().cardInfo);
        
        card.GetComponent<CardMover>().MoveToDestination(hasAuthority, to);
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


    [ClientRpc]
    public void RpcDiscardSelection(){
        foreach(var card in _discardSelection){
            var cardInfo = card.GetComponent<CardStats>().cardInfo;

            cards.hand.Remove(cardInfo);
            cards.discard.Add(cardInfo);
            // print("Discarding card: " + _cardInfo.title);

            RpcMoveCard(card, CardLocations.Hand, CardLocations.Discard);
        }
        
        _discardSelection.Clear();
    }
    
    // All those targets dont do much
    [TargetRpc]
    public void TargetRecruit(NetworkConnection target, int nbRecruits){

        print("Recruiting: " + nbRecruits);
        _kingdom.MaxButton();
    }

    [Command]
    public void CmdRecruitSelection(CardInfo card){
        
        if (card.title != null) Cash -= card.cost;
        
        Recruits--;
        TurnManager.Instance.PlayerSelectedRecruitCard(this, card);
    }

    [TargetRpc]
    public void TargetFinishRecruiting(){
        
        _kingdom.MinButton();

        foreach (var zone in _playZones)
        {
            bool auth = zone.gameObject.name == "PlayerDropZone";
            zone.DiscardMoneyCards(auth);
        }

        // foreach (var card in cards.hand)
        // {
        //     card.
        // }
    }

    #endregion TurnActions

    #region UI
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
        if(hasAuthority) playerUI.SetCash(value);
        else opponentUI.SetCash(value);
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
        if(hasAuthority) playerUI.SetRecruits(value);
        else opponentUI.SetRecruits(value);
    }
    #endregion UI

}