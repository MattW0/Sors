using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    [Header("Entities")]
    private GameManager _gameManager;
    private Kingdom _kingdom;
    public PlayerManager opponent;

    [Header("Game Stats")]
    public CardCollection cards;
    [SyncVar, SerializeField] private int _health;
    [SyncVar, SerializeField] private int _score;
    public int Health { get => _health; set => _health = value; }
    public int Score { get => _score; set => _score = value; }

    public List<Phase> playerChosenPhases = new List<Phase>() {Phase.DrawI, Phase.DrawII};
    private List<GameObject> _discardSelection = new List<GameObject>();

    [Header("Turn Stats")]
    [SyncVar, SerializeField] private int _cash = 0;
    public int Cash { 
        get => _cash; 
        set{
            _cash = value;
            SetCashValue(_cash);
            OnCashChanged?.Invoke(_cash);
        }
    }
    [SyncVar, SerializeField] private int _recruits = 1;
    public int Recruits { 
        get => _recruits; 
        set{
            _recruits = value;
            SetRecruitValue(_recruits);
            // OnCashChanged?.Invoke(_cash);
        }
    }

    public PlayerUI playerUI;
    public PlayerUI opponentUI;

    public static event Action OnCardPileChanged;
    public static event Action<int> OnCashChanged;

    #region GameSetup
    public override void OnStartClient(){
        base.OnStartClient();

        cards = GetComponent<CardCollection>();
        if (isServer) _gameManager = GameManager.Instance;
    }

    [ClientRpc]
    public void RpcFindOpponent(bool debug){   
        if(!hasAuthority) return;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        if(!debug) opponent = players[0] == this ? players[1] : players[0];
    }

    [ClientRpc]
    public void RpcSetPlayerStats(int startHealth, int startScore){   
        string name = isServer ? "Server" : "Client";
        string opponentName = !isServer ? "Server" : "Client"; // inverse of my name
        
        playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();

        if (hasAuthority){
            _health = startHealth;
            _score = startScore;
            playerUI.SetPlayerUI(name, startHealth.ToString(), startScore.ToString());
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

    public void DrawCards(int _amount){

        if(!isServer) return;

        while (_amount > cards.deck.Count + cards.discard.Count){
            _amount--;
        } 

        for (int i = 0; i < _amount; i++){
            if (cards.deck.Count == 0) ShuffleDiscardIntoDeck();

            CardInfo card = cards.deck[0];
            cards.deck.RemoveAt(0);
            cards.hand.Add(card);

            GameObject cardObject = _gameManager.GetCardObject(card.goID);
            RpcMoveCard(cardObject, "Hand");
        }
    }

    private void ShuffleDiscardIntoDeck(){
        Debug.Log("Shuffling discard into deck");

        List<CardInfo> temp = new List<CardInfo>();
        foreach (CardInfo _card in cards.discard){
            temp.Add(_card);
            cards.deck.Add(_card);

            GameObject _cachedCard = _gameManager.GetCardObject(_card.goID);
            RpcMoveCard(_cachedCard, "DrawPile");

        }

        foreach (CardInfo _card in temp){
            cards.discard.Remove(_card);
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

    public void PlayCard(GameObject card){
        if (isServer) RpcMoveCard(card, "PlayZone");
        else CmdMoveCard(card, "PlayZone");
    }

    [Command]
    private void CmdMoveCard(GameObject card, string destination){
        RpcMoveCard(card, destination);
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, string destination){
        card.GetComponent<CardMover>().MoveToDestination(hasAuthority, destination);
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
    public void CmdDiscardSelection(List<GameObject> cards){
        _discardSelection = cards;
        TurnManager.Instance.PlayerSelectedDiscardCards();
    }


    [ClientRpc]
    public void RpcDiscardSelection(){
        foreach(GameObject _card in _discardSelection){
            CardInfo _cardInfo = _card.GetComponent<CardStats>().cardInfo;

            cards.hand.Remove(_cardInfo);
            cards.discard.Add(_cardInfo);
            // print("Discarding card: " + _cardInfo.title);

            RpcMoveCard(_card, "DiscardPile");
        }
        
        _discardSelection.Clear();
    }

    [TargetRpc]
    public void TargetRecruit(NetworkConnection target, int nbRecruits){

        print("Recruiting: " + nbRecruits);

        if (_kingdom == null) _kingdom = Kingdom.Instance;
        _kingdom.MaxButton();
    }

    [Command]
    public void CmdRecruitSelection(CardInfo card){
        Recruits--;
        TurnManager.Instance.PlayerSelectedRecruitCard(this, card);
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