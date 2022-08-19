using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    private bool debug = false;

    private GameManager _gameManager;
    public PlayerManager opponent;
    public CardCollection cards;
    public PlayerUI playerUI;
    public PlayerUI opponentUI;

    [Header("Game Stats")]
    [SyncVar, SerializeField] private int _health;
    public int _score;
    public List<Phase> playerChosenPhases = new List<Phase>() {Phase.DrawI, Phase.DrawII};

    [Header("Turn Stats")]
    [SyncVar] private int _cash;
    public int Cash { get => _cash; set => _cash = value; }
    // [SyncVar] private int _cash;
    // [SyncVar] private int _cash;
    private List<GameObject> _discardSelection = new List<GameObject>();

    #region GameSetup
    public override void OnStartClient(){
        base.OnStartClient();

        if (isServer) {
            _gameManager = GameManager.Instance;
            debug = _gameManager.debug;
        }

        cards = GetComponent<CardCollection>();

        int numberPlayersRequired = debug ? 1 : 2;
        if (NetworkServer.connections.Count == numberPlayersRequired){
            RpcGameSetup();
        }
    }

    [ClientRpc]
    public void RpcGameSetup(){        
        // Disable Mirror HUD
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().enabled = false;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        if(!debug) opponent = players[0] == this ? players[1] : players[0];

        if (isServer){
            TurnManager.OnTurnStateChanged += RpcTurnChanged;
            GameManager.Instance.GameSetup();
        }
    }

    [ClientRpc]
    public void RpcSetUI(int startHealth, int startScore){   
        string name = isServer ? "Server" : "Client";
        string opponentName = !isServer ? "Server" : "Client"; // inverse of my name
        
        playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
        opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();

        if (hasAuthority){
            // playerUIPanel.SetActive(true);
            playerUI.isMine = true;
            playerUI.playerName.text = name;
            playerUI.playerHealth.text = startHealth.ToString();
            playerUI.playerScore.text = startScore.ToString();
            playerUI.readyButton.gameObject.SetActive(true);

            _health = startHealth;
            _score = startScore;
        } else {
            // opponentUIPanel.SetActive(true);
            opponentUI.playerName.text = opponentName;
            opponentUI.playerHealth.text = startHealth.ToString();
            opponentUI.playerScore.text = startScore.ToString();
        }
    }
    #endregion GameSetup

    #region Cards

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

    public void PlayCard(GameObject card){
        if (isServer){
            RpcMoveCard(card, "PlayZone");
        } else {
            CmdMoveCard(card, "PlayZone");
        }
    }

    [Command]
    private void CmdMoveCard(GameObject card, string destination){
        RpcMoveCard(card, destination);
    }

    [ClientRpc]
    public void RpcMoveCard(GameObject card, string destination)
    {
        card.GetComponent<CardMover>().MoveToDestination(hasAuthority, destination);
    }

    #endregion Cards

    #region TurnActions

    [ClientRpc]
    private void RpcTurnChanged(TurnState _state)
    {
        // print($"<color=LightSkyBlue>Turn changed to {_state}</color>");
    }

    // !!! workarounds to communicate with server !!!

    [Command] 
    public void CmdPhaseSelection(List<Phase> _phases){

        // Saving local player choice
        playerChosenPhases[0] = _phases[0];
        playerChosenPhases[1] = _phases[1];

        TurnManager.Instance.PlayerSelectedPhases(_phases);
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> _cards){
        _discardSelection = _cards;
        TurnManager.Instance.PlayerSelectedDiscardCards();
    }

    [ClientRpc]
    public void RpcDiscardSelection(){
        foreach(GameObject _card in _discardSelection){
            CardInfo _cardInfo = _gameManager.GetCardInfo(_card.name);

            cards.hand.Remove(_cardInfo);
            cards.discard.Add(_cardInfo);
            // print("Discarding card: " + _cardInfo.title);

            RpcMoveCard(_card, "DiscardPile");
        }

        _discardSelection.Clear();
    }

    #endregion TurnActions

    private void OnDestroy() {
        TurnManager.OnTurnStateChanged -= RpcTurnChanged;
    }
}