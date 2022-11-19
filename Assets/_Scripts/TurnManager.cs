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
    private PhasePanel _phasePanel;

    private PlayerHandManager _handManager;
    private BoardManager _boardManager;
    [SerializeField] private CombatManager combatManager;

    [field: Header("Game state")] 
    [SerializeField] private TurnState turnState;
    public List<Phase> chosenPhases;
    private static int _playersReady;
    private Dictionary<PlayerManager, int> _playerHealth = new();
    public int GetHealth(PlayerManager player) => _playerHealth[player];

    [Header("Helper Fields")]
    private Dictionary<PlayerManager, List<CardInfo>> _recruitedCards;
    
    // Events
    public static event Action<TurnState> OnPhaseChanged;
    public static event Action OnTurnsStarting;
    public static event Action<PlayerManager> OnPlayerDies;


    [Header("Objects")]
    [SerializeField] private GameObject phasePanelPrefab;
    // [SerializeField] private GameObject _discardPanelPrefab;

    private void Awake() {
        if (Instance == null) Instance = this;
        
        GameManager.OnGameStart += Prepare;
    }

    private void UpdateTurnState(TurnState newState){
        turnState = newState;

        if (newState != TurnState.NextPhase) {
            print($"<color=aqua>Turn changed to {newState}</color>");
        }

        switch(turnState){
            // --- Preparation and transition ---
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
                DrawIi();
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
            case TurnState.Idle:
                print("Game Ends");
                break;
            default:
                print("<color=red>Invalid turn state</color>");
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Prepare() {
        _gameManager = GameManager.Instance;
        _handManager = PlayerHandManager.Instance;
        _boardManager = BoardManager.Instance;
        _kingdom = Kingdom.Instance;
        
        // Panels with setup (GameManager handles kingdom setup)
        _discardPanel = DiscardPanel.Instance;
        _discardPanel.RpcPrepareDiscardPanel(_gameManager.nbDiscard);
        _phasePanel = PhasePanel.Instance;
        _phasePanel.RpcPreparePhasePanel(_gameManager.nbPhasesToChose, _gameManager.animations);
        
        foreach (var player in _gameManager.players.Keys)
        {
            _playerHealth.Add(player, _gameManager.startHealth);
        }

        PlayerManager.OnCashChanged += PlayerCashChanged;
        
        OnTurnsStarting?.Invoke();
        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    #region PhaseSelection
    private void PhaseSelection() {
        _gameManager.turnNb++;
        _phasePanel.RpcBeginPhaseSelection(_gameManager.turnNb);

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
        
        _phasePanel.RpcEndPhaseSelection();
        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase(){

        _playersReady = 0;
        
        if (chosenPhases.Count == 0) {
            UpdateTurnState(TurnState.CleanUp);
            OnPhaseChanged?.Invoke(TurnState.CleanUp);
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
    
    private void DrawIi(){
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
        
        // Reset to account for entities that died last turn
        _boardManager.ResetHolders();
        _boardManager.ShowCardPositionOptions(true);
    }

    public void PlayerDeployedCard(PlayerManager player, GameObject card, int holderNumber) {
        
        // If player did not skip deploy (and deployed a card)
        if (card)
        {
            var cardInfo = card.GetComponent<CardStats>().cardInfo;
            _gameManager.SpawnFieldEntity(player, card, cardInfo, holderNumber);
            player.Cash -= cardInfo.cost;
        }
        player.Deploys--;
        
        // Waiting for player to use other deploys
        if (player.Deploys > 0) return;
        
        _handManager.TargetHighlightMoney(_gameManager.players[player].connectionToClient, false);
        _playersReady++;
        print($"{_playersReady}/{_gameManager.players.Count} players ready");
        
        // Waiting for a player to finish recruiting
        if (_playersReady != _gameManager.players.Count) return;
        
        EndDeploy();
    }

    private void EndDeploy()
    {
        print("Everybody deployed");

        _handManager.RpcResetDeployability();
        PlayersStatsResetAndDiscardMoney();
        _boardManager.ShowCardPositionOptions(false);
        
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
                _gameManager.SpawnCreature(owner, cardInfo);
                print("<color=white>" + owner.PlayerName + " recruits " + cardInfo.title + "</color>");
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

        var gameEnds = false;
        foreach (var (player, health)  in _playerHealth)
        {
            if (health > 0) continue;
            
            OnPlayerDies?.Invoke(player);
            gameEnds = true;
        }

        if (gameEnds)
        {
            _gameManager.EndGame();
            UpdateTurnState(TurnState.Idle);
            return;
        }
        
        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    // helper functions
    public void PlayerHealthChanged(PlayerManager player, int amount)
    {
        _playerHealth[player] -= amount;
    }
    
    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        switch (turnState) {
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
        _boardManager.DiscardMoney();
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

    public void PlayerPressedReadyButton(PlayerManager player)
    {
        if (player.Recruits <= 0 || player.Deploys <= 0) return;
        
        switch (turnState)
        {
            case TurnState.Deploy:
                PlayerDeployedCard(player, null, -1);
                break;
            case TurnState.Combat:
                combatManager.PlayerIsReady(player);
                break;
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
    }
}

public enum TurnState
{
    Idle,
    NextPhase,
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
