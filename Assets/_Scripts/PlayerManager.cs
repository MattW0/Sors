using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    private bool debug = false;
    public GameObject cardPrefab;
    GameManager gameManager;
    public PlayerManager opponent;
    public CardCollection cards;
    public PlayerUI playerUI;
    public PlayerUI opponentUI;


    [Header("GameStats")]
    public int _health;
    public int _score;

    [Header("UI")]
    // [SerializeField] private GameObject phaseSelectionPanel;
    private Transform playerHand;
    private Transform playerDropZone;
    private Transform playerDrawPile;
    private Transform playerDiscardPile;
    private Transform opponentHand;
    private Transform opponentDropZone;
    private Transform opponentDrawPile;
    private Transform opponentDiscardPile;

    #region GameSetup
    public override void OnStartClient()
    {
        base.OnStartClient();

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
        Debug.Log("Game starting");
        GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().enabled = false;

        PlayerManager[] players = FindObjectsOfType<PlayerManager>();

        if(!debug) opponent = players[0] == this ? players[1] : players[0];

        TurnManager.OnTurnStateChanged += RpcTurnChanged;

        if (isServer){
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.GameSetup();
        }
    }

    [ClientRpc]
    public void RpcSetUI(int startHealth, int startScore)
    {   
        string name = isServer ? "Server" : "Client";
        string opponentName = !isServer ? "Server" : "Client"; // inverse of my name

        if (hasAuthority){
            // playerUIPanel.SetActive(true);
            playerUI = GameObject.Find("PlayerInfo").GetComponent<PlayerUI>();
            playerUI.isMine = true;
            playerUI.playerName.text = name;
            playerUI.playerHealth.text = startHealth.ToString();
            playerUI.playerScore.text = startScore.ToString();

            _health = startHealth;
            _score = startScore;
        } else {
            // opponentUIPanel.SetActive(true);
            opponentUI = GameObject.Find("OpponentInfo").GetComponent<PlayerUI>();
            opponentUI.playerName.text = opponentName;
            opponentUI.playerHealth.text = startHealth.ToString();
            opponentUI.playerScore.text = startScore.ToString();
        }
    }

    public void SpawnCard(ScriptableCard card)
    {
        GameObject cardObject = Instantiate(cardPrefab);
        string instanceID = cardObject.GetInstanceID().ToString();
        cardObject.name = instanceID;
        NetworkServer.Spawn(cardObject, connectionToClient);

        CardInfo cardInfo = new CardInfo(card, instanceID);
        cards.deck.Add(cardInfo);
        cardObject.GetComponent<CardUI>().RpcSetCardUI(cardInfo);

        RpcMoveCard(cardObject, "DrawPile");
    }
    #endregion

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
                card.GetComponent<DragDrop>().ChangeDragPermission(true);
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

    #region Turn actions

    [ClientRpc]
    private void RpcTurnChanged(TurnState state)
    {
        Debug.Log("Turn changed to " + state);
        // phaseSelectionPanel.SetActive(state == TurnState.PhaseSelection);
    }

    #endregion

    void OnDestroy() {
        TurnManager.OnTurnStateChanged -= RpcTurnChanged;
    }
}
