using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Entities")]
    private GameManager _gameManager;
    private PlayerManager _serverPlayer;
    private Kingdom _kingdom;
    private DiscardPanel _discardPanel;
    private PlayerHandManager _handManager;
    private List<PlayZoneManager> _playZoneManagers;
    
    [Header("Turn state")]
    [SerializeField] private TurnState state;
    public List<Phase> chosenPhases = new List<Phase>();
    private Dictionary<PlayerManager, List<CardInfo>> _recruitedCards;
    private static int _playersReady = 0;

    // Events
    public static event Action<TurnState> OnTurnStateChanged;

    [Header("Objects")]
    [SerializeField] private GameObject _phasePanelPrefab;
    private GameObject _phasePanel;
    // [SerializeField] private GameObject _discardPanelPrefab;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void UpdateTurnState(TurnState newState){
        state = newState;

        if (newState != TurnState.NextPhase) {
            print($"<color=aqua>Turn changed to {newState}</color>");
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
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnTurnStateChanged?.Invoke(state);
    }

    private void Prepare() {
        _gameManager = GameManager.Instance;
        _serverPlayer = _gameManager.players.Keys.First();
        _kingdom = Kingdom.Instance;
        
        _discardPanel = DiscardPanel.Instance;
        _discardPanel.RpcSetInactive(); // Must do this for clients
        
        _handManager = PlayerHandManager.Instance;
        _playZoneManagers = new List<PlayZoneManager>();
        _playZoneManagers.AddRange(FindObjectsOfType<PlayZoneManager>());
        foreach (var pzm in _playZoneManagers) pzm.Prepare();

        PlayerManager.OnCashChanged += PlayerCashChanged;

        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    #region PhaseSelection
    private void PhaseSelection() {
        _gameManager.turnNb++;

        _phasePanel = Instantiate(_phasePanelPrefab, transform);
        NetworkServer.Spawn(_phasePanel, connectionToClient);

        _playersReady = 0;
    }

    public void PlayerSelectedPhases(List<Phase> phases) {
        
        _playersReady++;
        foreach (var phase in phases) {
            if(!chosenPhases.Contains(phase)){
                chosenPhases.Add(phase);
            }
        }
        
        if (_playersReady != _gameManager.players.Count) return;

        // Starting the selected phases
        NetworkServer.Destroy(_phasePanel);
        chosenPhases.Sort();
        print($"<color=white>Chosen Phases: {string.Join(", ", chosenPhases)}</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase(){

        _playersReady = 0;
        
        if (chosenPhases.Count == 0) {
            UpdateTurnState(TurnState.CleanUp);
            return;
        }

        Enum.TryParse(chosenPhases[0].ToString(), out TurnState nextPhase);
        chosenPhases.RemoveAt(0);
        UpdateTurnState(nextPhase);
    }
    #endregion

    #region Drawing
    
    private void DrawI() {
        foreach (var player in _gameManager.players.Keys) {
            var nbCardDraw = _gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawI)) nbCardDraw++;

            player.DrawCards(nbCardDraw);
        }

        UpdateTurnState(TurnState.Discard);
    }
    
    private void DrawII(){
        foreach (var player in _gameManager.players.Keys) {
            var nbCardDraw = _gameManager.nbCardDraw;
            if (player.playerChosenPhases.Contains(Phase.DrawII)) nbCardDraw++;

            player.DrawCards(nbCardDraw);
        }
        
        UpdateTurnState(TurnState.Discard);
    }

    private void Discard() {
        _playersReady = 0;
        
        _handManager.RpcHighlightAll(true);
        _discardPanel.RpcBeginDiscard();
    }

    public void PlayerSelectedDiscardCards(){
        _playersReady++;
        if (_playersReady != _gameManager.players.Count) return;

        foreach (var player in _gameManager.players.Keys) {
            player.DiscardSelection();
        }

        _discardPanel.RpcFinishDiscard();
        _handManager.RpcHighlightAll(false);
        
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #endregion

    private void Develop(){
        print("<color=yellow>Develop not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void Deploy()
    {
        _playersReady = 0;
        _handManager.RpcHighlightMoney(true);
        
        // Bonus for phase selection
        foreach (var playerManager in _gameManager.players.Keys) {
            var nbDeploys = _gameManager.turnDeploys;
            if (playerManager.playerChosenPhases.Contains(Phase.Deploy)) nbDeploys++;
            playerManager.Deploys = nbDeploys;
        }
    }

    public void PlayerSelectedDeploy(PlayerManager player, CardInfo card) {
        // If player did not skip deploy (and clicked a card)
        if (card.title != null) player.PlayCard(_gameManager.GetCardObject(card.goID), false);
        
        // Waiting for player to use other deploys
        if (player.Deploys > 0) return;
        
        _playersReady++;
        print($"{_playersReady}/{_gameManager.players.Count} players ready");
        
        // Waiting for a player to finish recruiting
        if (_playersReady != _gameManager.players.Count) return;
        print("Everybody recruited");
        // NextPhase();
    }

    private void Combat(){
        print("<color=yellow>Combat not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #region Recruit

    private void Recruit(){
        _recruitedCards = new Dictionary<PlayerManager, List<CardInfo>>();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginRecruit();

        foreach (var playerManager in _gameManager.players.Keys) {
            var nbRecruits = _gameManager.turnRecruits;
            if (playerManager.playerChosenPhases.Contains(Phase.Recruit)) nbRecruits++;
            playerManager.Recruits = nbRecruits;
        }
    }

    public void PlayerSelectedRecruitCard(PlayerManager player, CardInfo card)
    {
        if (card.title != null) // If player did not skip recruit (and selected a card)
        {
            if (_recruitedCards.ContainsKey(player))
                _recruitedCards[player].Add(card);
            else
                _recruitedCards.Add(player, new List<CardInfo> { card });
        }
        
        _kingdom.TargetResetRecruit(_gameManager.players[player].connectionToClient, player.Recruits);

        // Waiting for player to use other recruits
        if (player.Recruits > 0) return;
        
        _playersReady++;
        print($"{_playersReady}/{_gameManager.players.Count} players ready");
        
        // Waiting for a player to finish recruiting
        if (_playersReady != _gameManager.players.Count) return;

        RecruitSpawnAndReset();
    }

    private void RecruitSpawnAndReset()
    {
        foreach (var (owner, cards) in _recruitedCards) {
            foreach (var cardInfo in cards) {
                _gameManager.SpawnCreature(owner, _gameManager.players[owner], cardInfo);
                print("<color=white>" + owner.playerName + " recruits " + cardInfo.title + "</color>");
            }
        }
        
        PlayersFinishRecruiting();

        foreach (var zone in _playZoneManagers)
        {
            zone.RpcDiscardMoney();
        }
        
        _kingdom.EndRecruit();
        _handManager.RpcHighlightMoney(false);

        UpdateTurnState(TurnState.NextPhase);
    }
    
    private void PlayersFinishRecruiting()
    {
        foreach (var owner in _gameManager.players.Keys)
        {
            owner.DiscardMoneyCards();
            
            owner.Recruits = _gameManager.turnRecruits;
            owner.Cash = _gameManager.turnCash;
            
            owner.moneyCards.Clear();
        }
    }
    #endregion 

    private void Prevail(){
        print("<color=yellow>Prevail not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void CleanUp(){
        print("<color=yellow>CleanUp not yet implemented</color>");
        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    // helper functions
    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        switch (state) {
            case TurnState.Deploy:
                _handManager.TargetCheckDeployability(_gameManager.players[player].connectionToClient, newAmount);
                break;
            case TurnState.Recruit:
                _kingdom.TargetCheckRecruitability(_gameManager.players[player].connectionToClient, newAmount);
                break;
        }
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
