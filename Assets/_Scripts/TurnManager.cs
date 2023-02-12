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

    private Hand _handManager;
    private BoardManager _boardManager;
    [SerializeField] private CombatManager combatManager;

    [field: Header("Game state")] 
    [SerializeField] public TurnState turnState { get; private set; }
    public List<Phase> phasesToPlay;
    private Dictionary<PlayerManager, Phase[]> _playerPhaseChoices = new();
    private List<PlayerManager> _readyPlayers = new();
    private int _nbPlayers;
    private Dictionary<PlayerManager, int> _playerHealth = new();
    public int GetHealth(PlayerManager player) => _playerHealth[player];

    [Header("Helper Fields")]
    private Dictionary<PlayerManager, List<CardInfo>> _selectedCards;
    
    // Events
    public static event Action<Phase[]> OnPhasesSelected;
    public static event Action<TurnState> OnPhaseChanged;
    public static event Action OnTurnsStarting;
    public static event Action<PlayerManager> OnPlayerDies;

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

    private void Prepare(int nbPlayers) {
        _gameManager = GameManager.Instance;
        _handManager = Hand.Instance;
        _boardManager = BoardManager.Instance;
        _kingdom = Kingdom.Instance;
        
        // Panels with setup (GameManager handles kingdom setup)
        _discardPanel = DiscardPanel.Instance;
        _discardPanel.RpcPrepareDiscardPanel(_gameManager.nbDiscard);
        _phasePanel = PhasePanel.Instance;
        _phasePanel.RpcPreparePhasePanel(_gameManager.nbPhasesToChose, _gameManager.animations);
        
        _nbPlayers = nbPlayers;
        foreach (var player in _gameManager.players.Keys)
        {
            _playerHealth.Add(player, _gameManager.startHealth);
            _playerPhaseChoices.Add(player, new Phase[_gameManager.nbPhasesToChose]);
        }
        // reverse order of _playerPhaseChoices to have host first
        _playerPhaseChoices = _playerPhaseChoices.Reverse().ToDictionary(x => x.Key, x => x.Value);

        PlayerManager.OnCashChanged += PlayerCashChanged;
        
        OnTurnsStarting?.Invoke();
        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    #region PhaseSelection
    private void PhaseSelection() {
        _gameManager.turnNb++;
        _phasePanel.RpcBeginPhaseSelection(_gameManager.turnNb);
    }

    public void PlayerSelectedPhases(PlayerManager player, Phase[] phases) {
        
        _playerPhaseChoices[player] = phases;

        foreach (var phase in phases) {
            if(!phasesToPlay.Contains(phase)){
                phasesToPlay.Add(phase);
            }
        }

        PlayerIsReady(player);
    }

    private void FinishPhaseSelection() {
        // Combat each round
        phasesToPlay.Add(Phase.Combat);
        phasesToPlay.Sort();
        print($"<color=white>Chosen Phases: {string.Join(", ", phasesToPlay)}</color>");
        
        _phasePanel.RpcEndPhaseSelection();

        // Give the player choices to PlayerInterfaceVisuals
        var choices = new Phase[_nbPlayers * _gameManager.nbPhasesToChose];
        var i = 0;
        foreach (var phases in _playerPhaseChoices.Values) {
            foreach (var p in phases) {
                choices[i] = p;
                i++;
            }
        }
        OnPhasesSelected?.Invoke(choices);
        
        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase(){

        _readyPlayers.Clear();

        if (phasesToPlay.Count == 0) {
            UpdateTurnState(TurnState.CleanUp);
            OnPhaseChanged?.Invoke(TurnState.CleanUp);
            return;
        }

        Enum.TryParse(phasesToPlay[0].ToString(), out TurnState nextPhase);
        phasesToPlay.RemoveAt(0);
        
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
        _handManager.RpcHighlightAll(true);
        _discardPanel.RpcBeginDiscard();
    }

    public void PlayerSelectedDiscardCards(PlayerManager player){
        PlayerIsReady(player);        
    }

    private void FinishDiscard() {
        foreach (var p in _readyPlayers) {
            p.DiscardSelection();
        }

        _discardPanel.RpcFinishDiscard();
        _handManager.RpcHighlightAll(false);
        
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #endregion

    #region Develop
    private void Develop(){
        _selectedCards = new Dictionary<PlayerManager, List<CardInfo>>();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginPhase(Phase.Develop);

        foreach (var player in _gameManager.players.Keys) {
            if (player.playerChosenPhases.Contains(Phase.Develop)){
                _kingdom.TargetDevelopBonus(player.connectionToClient, _gameManager.developPriceReduction);
            } 
        }
    }

    public void PlayerSelectedDevelopCard(PlayerManager player, CardInfo card)
    {
        var cost = card.cost;
        if (player.playerChosenPhases.Contains(Phase.Develop)) cost -= _gameManager.developPriceReduction;
        player.Cash -= cost;

        if (_selectedCards.ContainsKey(player))
            _selectedCards[player].Add(card);
        else
            _selectedCards.Add(player, new List<CardInfo> { card });
        
        PlayerIsReady(player);
    }

    private void DevelopSpawnAndReset()
    {
        foreach (var (owner, cards) in _selectedCards) {
            foreach (var cardInfo in cards) {
                print("<color=white>" + owner.PlayerName + " develops " + cardInfo.title + "</color>");
                _gameManager.SpawnMoney(owner, cardInfo);
            }
        }
        
        PlayersStatsResetAndDiscardMoney();
        _kingdom.RpcEndDevelop();

        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion
    
    #region Deploy

    private void Deploy()
    {
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
        
        player.Deploys--;
        var cardInfo = card.GetComponent<CardStats>().cardInfo;
        player.Cash -= cardInfo.cost;
        _gameManager.SpawnFieldEntity(player, card, cardInfo, holderNumber);

        // Waiting for player to use other deploys
        if (player.Deploys > 0) return;
        _handManager.TargetHighlightMoney(player.connectionToClient, false);
        
        // Waiting for a player to finish recruiting
        PlayerIsReady(player);
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
        _selectedCards = new Dictionary<PlayerManager, List<CardInfo>>();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginPhase(Phase.Recruit);

        foreach (var playerManager in _gameManager.players.Keys) {
            var nbRecruits = _gameManager.turnRecruits;
            if (playerManager.playerChosenPhases.Contains(Phase.Recruit)) nbRecruits++;
            playerManager.Recruits = nbRecruits;
        }
    }

    public void PlayerSelectedRecruitCard(PlayerManager player, CardInfo card)
    {
        player.Recruits--;
        player.Cash -= card.cost;
        if (_selectedCards.ContainsKey(player))
            _selectedCards[player].Add(card);
        else
            _selectedCards.Add(player, new List<CardInfo> { card });
        
        _kingdom.TargetResetRecruit(player.connectionToClient, player.Recruits);

        // Waiting for player to use remaining recruit actions
        if (player.Recruits > 0) return;
        
        PlayerIsReady(player);
    }

    private void RecruitSpawnAndReset()
    {
        foreach (var (owner, cards) in _selectedCards) {
            foreach (var cardInfo in cards) {
                _gameManager.SpawnCreature(owner, cardInfo);
                print("<color=white>" + owner.PlayerName + " recruits " + cardInfo.title + "</color>");
            }
        }
        
        PlayersStatsResetAndDiscardMoney();
        _kingdom.RpcEndRecruit();

        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion 

    private void Prevail(){
        print("<color=yellow>Prevail not yet implemented</color>");
        UpdateTurnState(TurnState.NextPhase);
    }

    private void CleanUp(){

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
        }
        
        _readyPlayers.Clear();
        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    #region HelperFunctions
    public void PlayerIsReady(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _nbPlayers) return;
        
        switch (turnState) {
            case TurnState.PhaseSelection:
                FinishPhaseSelection();
                break;
            case TurnState.Discard:
                FinishDiscard();
                break;
            case TurnState.Develop:
                DevelopSpawnAndReset();
                break;
            case TurnState.Deploy:
                player.Deploys = 0;
                EndDeploy();
                break;
            case TurnState.Recruit:
                RecruitSpawnAndReset();
                break;
        }
    }

    public void PlayerHealthChanged(PlayerManager player, int amount)
    {
        _playerHealth[player] -= amount;
    }
    
    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        switch (turnState) {
            case TurnState.Develop:
                _kingdom.TargetCheckDevelopability(player.connectionToClient, newAmount);
                break;
            case TurnState.Deploy:
                _handManager.TargetCheckDeployability(player.connectionToClient, newAmount);
                break;
            case TurnState.Recruit:
                _kingdom.TargetCheckRecruitability(player.connectionToClient, newAmount);
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
    #endregion

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
