using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Entities")]
    private GameManager _gameManager;
    private Kingdom _kingdom;
    
    [Header("Turn state")]
    [SerializeField] private TurnState state;
    public List<Phase> chosenPhases = new List<Phase>();
    private Dictionary<PlayerManager, CardInfo> _recruitedCards;
    private static int playersReady = 0;

    // Events
    public static event Action<TurnState> OnTurnStateChanged;

    [Header("Objects")]
    [SerializeField] private GameObject _phasePanelPrefab;
    private GameObject _phasePanel;
    [SerializeField] private GameObject _discardPanelPrefab;
    private GameObject _discardPanel;

    void Awake() {
        if (Instance == null) Instance = this;
    }

    public void UpdateTurnState(TurnState _newState){
        state = _newState;

        if (_newState != TurnState.NextPhase) {
            print($"<color=aqua>Turn changed to {_newState}</color>");
        }

        switch(state){
            // --- Preparation and transition ---
            case TurnState.Prepare:
                Prepare();
                break;
            case TurnState.PhaseSelection:
                PhaseSelection();
                break;
            case TurnState.NextPhase:
                NextPhase();
                break;
            // --- Phases ---
            case TurnState.DrawI:
                DrawI();
                break;
            case TurnState.Discard:
                Discard();
                break;
            case TurnState.Develop:
                Develop();
                break;
            case TurnState.Deploy:
                Deploy();
                break;
            case TurnState.Combat:
                Combat();
                break;
            case TurnState.DrawII:
                DrawII();
                break;
            case TurnState.Recruit:
                Recruit();
                break;
            case TurnState.Prevail:
                Prevail();
                break;
            // --- Win check and turn reset ---
            case TurnState.CleanUp:
                CleanUp();
                break;
            default:
                print("<color=red>Invalid turn state</color>");
                throw new ArgumentOutOfRangeException(nameof(_newState), _newState, null);
        }

        OnTurnStateChanged?.Invoke(state);
    }

    private void Prepare() {
        _gameManager = GameManager.Instance;
        _kingdom = Kingdom.Instance;

        UpdateTurnState(TurnState.PhaseSelection);
    }
 
    private void PhaseSelection() {

        _gameManager.turnNb++;

        _phasePanel = Instantiate(_phasePanelPrefab, transform);
        NetworkServer.Spawn(_phasePanel, connectionToClient);

        playersReady = 0;
    }

    public void PlayerSelectedPhases(List<Phase> phases) {
        
        playersReady++;
        foreach (Phase phase in phases) {
            if(!chosenPhases.Contains(phase)){
                chosenPhases.Add(phase);
            }
        }
        
        if (!(playersReady == _gameManager.players.Count)) return;

        // Starting the selected phases
        NetworkServer.Destroy(_phasePanel);
        chosenPhases.Sort();
        print($"<color=white>Chosen Phases: {string.Join(", ", chosenPhases)}</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase(){

        playersReady = 0;
        
        if (chosenPhases.Count == 0) {
            UpdateTurnState(TurnState.CleanUp);
            return;
        }

        Enum.TryParse(chosenPhases[0].ToString(), out TurnState nextPhase);
        chosenPhases.RemoveAt(0);
        UpdateTurnState(nextPhase);
    }

    private void DrawI() {
        foreach (PlayerManager player in _gameManager.players) {
            int _nbCardDraw = _gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawI)) _nbCardDraw++;

            player.DrawCards(_nbCardDraw);
        }

        UpdateTurnState(TurnState.Discard);
    }

    private void Discard() {

        playersReady = 0;
        _discardPanel = Instantiate(_discardPanelPrefab, transform);
        NetworkServer.Spawn(_discardPanel, connectionToClient);

        // foreach (PlayerManager player in gameManager.players) {
        //     NetworkIdentity nwIdentity = player.gameObject.GetComponent<NetworkIdentity>();
        //     player.TargetDiscardCards(nwIdentity.connectionToClient, gameManager.nbDiscard);
        // }
    }

    public void PlayerSelectedDiscardCards(){
        playersReady++;
        if (!(playersReady == _gameManager.players.Count)) return;

        foreach (PlayerManager player in _gameManager.players) {
            player.RpcDiscardSelection();
        }

        NetworkServer.Destroy(_discardPanel);
        Destroy(_discardPanel);
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Develop(){
        print("<color=yellow>Develop not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Deploy(){
        print("<color=yellow>Deploy not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Combat(){
        print("<color=yellow>Combat not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void DrawII(){
        foreach (PlayerManager player in _gameManager.players) {
            int _nbCardDraw = _gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawII)) _nbCardDraw++;

            player.DrawCards(_nbCardDraw);
        }
        
        UpdateTurnState(TurnState.Discard);
    }

    private void Recruit(){

        _recruitedCards = new Dictionary<PlayerManager, CardInfo>();

        foreach (PlayerManager player in _gameManager.players) {
            
            NetworkIdentity targetPlayer = player.GetComponent<NetworkIdentity>();
            int nbRecruits = _gameManager.turnRecruits;

            // Allowing money to be played
            RecruitHighlightMoney(player, targetPlayer);
            if (player.playerChosenPhases.Contains(Phase.Recruit)) nbRecruits++;

            player.TargetRecruit(targetPlayer.connectionToClient, nbRecruits);
        }
    }

    private void RecruitHighlightMoney(PlayerManager player, NetworkIdentity targetPlayer) {
        foreach (CardInfo card in player.cards.hand) {
            if (card.isCreature) continue;

            GameObject cardObject = _gameManager.GetCardObject(card.goID);
            cardObject.GetComponent<CardStats>().TargetSetInteractable(targetPlayer.connectionToClient, true);
        }
    }

    public void PlayerSelectedRecruitCard(PlayerManager player, CardInfo card){

        player.Cash -= card.cost;
        print("Player " + player + " bought " + card.title);
        // CANT HAVE THE SAME KEY TWICE! -> change to <PlayerManager, CardInfo[]>
        _recruitedCards.Add(player, card);
        if (player.Recruits > 0) return;

        playersReady++;
        if (!(playersReady == _gameManager.players.Count)) return;

        foreach ((PlayerManager owner, CardInfo _card) in _recruitedCards) {
            print("<color=white>Recruited card: " + _card.title + "</color>");
            print("<color=white>By player: " + owner + "</color>");
            _gameManager.SpawnCreature(owner, _card);
        }
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Prevail(){
        print("<color=yellow>Prevail not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void CleanUp(){

        print("<color=yellow>CleanUp not yet implemented</color>");
        UpdateTurnState(TurnState.PhaseSelection);
    }
}

public enum TurnState
{
    Prepare,
    NextPhase,
    WaitingForReady,
    PhaseSelection,
    DrawI,
    Discard,
    Develop,
    Deploy,
    Combat,
    DrawII,
    Recruit,
    Prevail,
    CleanUp
}

public enum Phase
{
    DrawI,
    Develop,
    Deploy,
    DrawII,
    Recruit,
    Prevail,
}
