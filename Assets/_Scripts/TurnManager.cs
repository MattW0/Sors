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
    private Kingdom _kingdom;
    private DiscardPanel _discardPanel;
    private PlayerHandManager _handManager;
    private List<PlayZoneManager> _playZoneManagers;
    [SerializeField] private CombatManager combatManager;
    
    [Header("Turn state")]
    [SerializeField] private TurnState state;
    public List<Phase> chosenPhases;
    private static int _playersReady;

    [Header("Helper Fields")]
    private Dictionary<PlayerManager, List<CardInfo>> _recruitedCards;
    
    // Events
    public static event Action<TurnState> OnPhaseChanged;

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
    }

    private void Prepare() {
        _gameManager = GameManager.Instance;
        _kingdom = Kingdom.Instance;
        
        _discardPanel = DiscardPanel.Instance;
        _discardPanel.RpcSetInactive(); // Must do this for clients
        
        _handManager = PlayerHandManager.Instance;
        _playZoneManagers = new List<PlayZoneManager>();
        _playZoneManagers.AddRange(FindObjectsOfType<PlayZoneManager>());

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

        // Combat each round
        chosenPhases.Add(Phase.Combat);
        chosenPhases.Sort();
        print($"<color=white>Chosen Phases: {string.Join(", ", chosenPhases)}</color>");
        
        NetworkServer.Destroy(_phasePanel);
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
        
        OnPhaseChanged?.Invoke(nextPhase);
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
    
    #region Deploy

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
        
        foreach (var zone in _playZoneManagers)
        {
            zone.RpcShowCardPositionOptions(true);
        }
    }

    public void PlayerDeployedCard(PlayerManager player, GameObject cardObject) {
        // If player did not skip deploy (and deployed a card)
        if (!cardObject) return;

        var card = cardObject.GetComponent<CardStats>().cardInfo;
        player.Deploys--;
        player.Cash -= card.cost;
        
        // Waiting for player to use other deploys
        if (player.Deploys > 0) return;
        
        _playersReady++;
        print($"{_playersReady}/{_gameManager.players.Count} players ready");
        
        // Waiting for a player to finish recruiting
        if (_playersReady != _gameManager.players.Count) return;
        
        EndDeploy();
    }

    private void EndDeploy()
    {
        print("Everybody deployed");

        PlayersStatsResetAndDiscardMoney();
        foreach (var zone in _playZoneManagers)
        {
            zone.RpcShowCardPositionOptions(false);
        }
        
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #endregion

    private void Combat()
    {
        combatManager.UpdateCombatState(CombatState.Attackers);
    }

    public void CombatCleanUp()
    {
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
        
        PlayersStatsResetAndDiscardMoney();
        _kingdom.EndRecruit();

        UpdateTurnState(TurnState.NextPhase);
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
    
    private void PlayersStatsResetAndDiscardMoney()
    {
        foreach (var zone in _playZoneManagers)
        {
            zone.RpcDiscardMoney();
        }
        _handManager.RpcHighlightMoney(false);

        foreach (var owner in _gameManager.players.Keys)
        {
            owner.DiscardMoneyCards();
            owner.moneyCards.Clear();
            
            owner.Cash = _gameManager.turnCash;
            owner.Deploys = _gameManager.turnDeploys;
            owner.Recruits = _gameManager.turnRecruits;
            
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
    Combat,
    DrawII,
    Recruit,
    Prevail,
}
