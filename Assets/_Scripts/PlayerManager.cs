using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    private bool debug = false;
    private GameManager gameManager;
    public PlayerManager opponent;
    public CardCollection cards;
    public PlayerUI playerUI;
    public PlayerUI opponentUI;
    // public GameObject cardPrefab;

    [Header("GameStats")]

    [SyncVar, SerializeField] private int _health;
    public int _score;
    public List<Phase> playerChosenPhases = new List<Phase>() {Phase.DrawI, Phase.DrawII};

    [Header("UI")]
    // [SerializeField] private GameObject phaseSelectionPanel;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform playerDropZone;
    [SerializeField] private Transform playerDrawPile;
    [SerializeField] private Transform playerDiscardPile;
    [SerializeField] private Transform opponentHand;
    [SerializeField] private Transform opponentDropZone;
    [SerializeField] private Transform opponentDrawPile;
    [SerializeField] private Transform opponentDiscardPile;

    #region GameSetup
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer) debug = GameManager.instance.debug;

        cards = GetComponent<CardCollection>();

        playerDrawPile = GameObject.Find("PlayerDrawPile").transform.GetChild(0);
        opponentDrawPile = GameObject.Find("OpponentDrawPile").transform.GetChild(0);
        playerHand = GameObject.Find("PlayerHand").transform;
        opponentHand = GameObject.Find("OpponentHand").transform;
        playerDropZone = GameObject.Find("PlayerDropZone").transform;
        opponentDropZone = GameObject.Find("OpponentDropZone").transform;

        int numberPlayersRequired = debug ? 1 : 2;
        if (NetworkServer.connections.Count == numberPlayersRequired){
            RpcGameSetup();
        }
    }

    [ClientRpc]
    public void RpcGameSetup()
    {
        // Debug.Log("Game starting");
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().enabled = false;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        if(!debug) opponent = players[0] == this ? players[1] : players[0];

        TurnManager.OnTurnStateChanged += RpcTurnChanged;

        if (isServer){
            gameManager = GameManager.instance;
            gameManager.GameSetup();
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

    // public void SpawnCard(ScriptableCard card)
    // {
    //     GameObject cardObject = Instantiate(cardPrefab);
    //     string instanceID = cardObject.GetInstanceID().ToString();
    //     cardObject.name = instanceID;
    //     NetworkServer.Spawn(cardObject, connectionToClient);

    //     CardInfo cardInfo = new CardInfo(card, instanceID);
    //     cards.deck.Add(cardInfo);
    //     cardObject.GetComponent<CardUI>().RpcSetCardUI(cardInfo);

    //     RpcMoveCard(cardObject, "DrawPile");
    // }

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
    // [ClientRpc]
    public void TargetDiscardCards(NetworkConnection target, int nbToDiscard){
        if (cards.hand.Count == 0) return;

        // foreach (CardUI _ui in playerHand.GetComponentsInChildren<CardUI>()){
        //     _ui.Highlight(true);
        // }

        foreach (CardUI _ui in playerHand.GetComponentsInChildren<CardUI>()){
            _ui.Highlight(true);
        }
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
        switch (destination){

        default:
            Debug.Log("Unknown card destination");
            break;
        case "DrawPile":
            if (hasAuthority) card.transform.SetParent(playerDrawPile, false);
            else card.transform.SetParent(opponentDrawPile, false);
            break;
        case "Hand":
            if (hasAuthority) {
                card.transform.SetParent(playerHand, false);
                card.GetComponent<CardUI>().Flip();
                // card.GetComponent<DragDrop>().ChangeDragPermission(true);
            }
            else card.transform.SetParent(opponentHand, false);
            break;
        case "PlayZone":
            if (hasAuthority) card.transform.SetParent(playerDropZone, false);
            else {
                card.transform.SetParent(opponentDropZone, false);
                card.GetComponent<CardUI>().Flip();
            }
            break;
        }
    }

    #endregion Cards

    #region TurnActions

    [ClientRpc]
    private void RpcTurnChanged(TurnState state)
    {
        Debug.Log("Turn changed to " + state);
        
        // if (state == TurnState.PhaseSelection) playerChosenPhases.Clear();
    }

    [Command] // workaround to communicate with server
    public void CmdPhaseSelection(List<Phase> _phases){

        // Saving local player choice
        playerChosenPhases[0] = _phases[0];
        playerChosenPhases[1] = _phases[1];

        TurnManager.instance.PlayerSelectedPhases(_phases);
    }

    [Command]
    public void CmdDiscardSelection(List<CardInfo> _cards){

        TurnManager.instance.PlayerSelectedDiscardCards();

        foreach(CardInfo _card in _cards){
            RpcMoveCard(GameObject.Find(_card.goID), "DiscardPile");
        }
    }

    #endregion TurnActions

    void OnDestroy() {
        TurnManager.OnTurnStateChanged -= RpcTurnChanged;
    }
}