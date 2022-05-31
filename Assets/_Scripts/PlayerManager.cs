using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    private bool debug = false;

    public PlayerManager opponent;
    public CardCollection cards;
    public PlayerUI playerUI;
    public PlayerUI opponentUI;

    [Header("GameStats")]
    [SyncVar, SerializeField] private int _health;
    public int _score;
    public List<Phase> playerChosenPhases = new List<Phase>() {Phase.DrawI, Phase.DrawII};

    #region GameSetup
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer) debug = GameManager.Instance.debug;

        cards = GetComponent<CardCollection>();

        int numberPlayersRequired = debug ? 1 : 2;
        if (NetworkServer.connections.Count == numberPlayersRequired){
            RpcGameSetup();
        }
    }

    [ClientRpc]
    public void RpcGameSetup()
    {        
        // Turning off Mirror "Menu"
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().enabled = false;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        if(!debug) opponent = players[0] == this ? players[1] : players[0];

        if (isServer){
            TurnManager.OnTurnStateChanged += RpcTurnChanged;
            GameManager.Instance.GameSetup();
        }
    }

    [ClientRpc]
    public void RpcSetUI(int startHealth, int startScore)
    {   
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

    public void DrawCard(){
        if (cards.deck.Count == 0){
            Debug.Log("No more cards in deck!");
            return;
            // TODO: Shuffle Discard pile -> Draw pile
        }

        CardInfo card = cards.deck[0];
        cards.deck.RemoveAt(0);
        cards.hand.Add(card);

        GameObject cardObject = GameObject.Find(card.goID);
        RpcMoveCard(cardObject, "Hand");
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
    public void CmdMoveCard(GameObject card, string destination){
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

    [Command] // workaround to communicate with server
    public void CmdPhaseSelection(List<Phase> _phases){

        // Saving local player choice
        playerChosenPhases[0] = _phases[0];
        playerChosenPhases[1] = _phases[1];

        TurnManager.Instance.PlayerSelectedPhases(_phases);
    }

    [Command]
    public void CmdDiscardSelection(List<GameObject> _cards){

        TurnManager.Instance.PlayerSelectedDiscardCards();

        foreach(GameObject _card in _cards){
            RpcMoveCard(_card, "DiscardPile");
        }
    }

    #endregion TurnActions

    private void OnDestroy() {
        TurnManager.OnTurnStateChanged -= RpcTurnChanged;
    }
}