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
    private CardCollectionPanel _cardCollectionPanel;
    private PhasePanel _phasePanel;
    private PrevailPanel _prevailPanel;
    private PlayerInterfaceManager _playerInterfaceManager;
    private Hand _handManager;
    private BoardManager _boardManager;
    [SerializeField] private CombatManager combatManager;

    [field: Header("Game state")] 
    [SerializeField] public TurnState turnState { get; private set; }
    public List<Phase> phasesToPlay;
    private Dictionary<PlayerManager, Phase[]> _playerPhaseChoices = new();
    private List<PrevailOption> _prevailOptionsToPlay = new();
    private Dictionary<PlayerManager, List<PrevailOption>> _playerPrevailOptions = new();
    private List<PlayerManager> _readyPlayers = new();
    private int _nbPlayers;
    private Dictionary<PlayerManager, int> _playerHealth = new();
    public int GetHealth(PlayerManager player) => _playerHealth[player];

    [Header("Helper Fields")]
    private Dictionary<PlayerManager, List<CardInfo>> _selectedKingdomCards = new();
    private Dictionary<PlayerManager, List<GameObject>> _selectedHandCards = new();
    
    // Events
    public static event Action<Phase[]> OnPhasesSelected;
    public static event Action<TurnState> OnPhaseChanged;
    public static event Action<PlayerManager> OnPlayerDies;

    private void Awake() {
        if (Instance == null) Instance = this;
        
        GameManager.OnGameStart += Prepare;
    }

    private void UpdateTurnState(TurnState newState){
        turnState = newState;

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
                _playerInterfaceManager.RpcLog("Game finished");
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
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
        
        // Panels with setup (GameManager handles kingdom setup)
        _cardCollectionPanel = CardCollectionPanel.Instance;
        _cardCollectionPanel.RpcPrepareCardCollectionPanel(_gameManager.nbDiscard);
        _phasePanel = PhasePanel.Instance;
        _phasePanel.RpcPreparePhasePanel(_gameManager.nbPhasesToChose, _gameManager.animations);
        _prevailPanel = PrevailPanel.Instance;
        _prevailPanel.RpcPreparePrevailPanel(_gameManager.prevailOptionsToChoose);

        _nbPlayers = nbPlayers;
        foreach (var player in _gameManager.players.Keys)
        {
            _playerHealth.Add(player, _gameManager.startHealth);
            _playerPhaseChoices.Add(player, new Phase[_gameManager.nbPhasesToChose]);
            _playerPrevailOptions.Add(player, new List<PrevailOption>());
            _selectedHandCards.Add(player, new List<GameObject>());
        }
        // reverse order of _playerPhaseChoices to have host first
        _playerPhaseChoices = _playerPhaseChoices.Reverse().ToDictionary(x => x.Key, x => x.Value);
        _playerPrevailOptions = _playerPrevailOptions.Reverse().ToDictionary(x => x.Key, x => x.Value);
        PlayerManager.OnCashChanged += PlayerCashChanged;

        UpdateTurnState(TurnState.PhaseSelection);
    }
    
    #region PhaseSelection
    private void PhaseSelection() {
        _gameManager.turnNb++;
        _playerInterfaceManager.RpcLog($"<color=#000142> ------------ Turn {_gameManager.turnNb}</color> ------------");
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
        _playerInterfaceManager.RpcLog($"<color=#383838>Phases to play: {string.Join(", ", phasesToPlay)}</color>");
        
        _phasePanel.RpcEndPhaseSelection();

        // Give the player choices to player turn stats and UI
        var choices = new Phase[_nbPlayers * _gameManager.nbPhasesToChose];
        var i = 0;
        foreach (var (player, phases) in _playerPhaseChoices) {
            foreach (var phase in phases){
                // Player turn stats
                if (phase == Phase.Deploy) player.Deploys++;
                else if(phase == Phase.Recruit) {
                    print("Recruit Bonus");
                    player.Recruits++;
                }
                // For phaseVisuals
                choices[i] = phase;
                i++;
            }
        }

        // Send choices to PhaseVisuals (via PlayerInterfaceManager)
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

        _playerInterfaceManager.RpcLog($"<color=#004208>Turn changed to {nextPhase}</color>");
        
        OnPhaseChanged?.Invoke(nextPhase);
        UpdateTurnState(nextPhase);
    }
    #endregion

    #region Drawing
    
    private void DrawI() {
        foreach (var player in _gameManager.players.Keys) {
            var nbCardDraw = _gameManager.nbCardDraw;
            if (player.chosenPhases.Contains(Phase.DrawI)) nbCardDraw++;

            player.DrawCards(nbCardDraw);
        }

        UpdateTurnState(TurnState.Discard);
    }
    
    private void DrawIi(){
        foreach (var player in _gameManager.players.Keys) {
            var nbCardDraw = _gameManager.nbCardDraw;
            if (player.chosenPhases.Contains(Phase.DrawII)) nbCardDraw++;

            player.DrawCards(nbCardDraw);
        }
        
        UpdateTurnState(TurnState.Discard);
    }

    private void Discard(){
        foreach(var player in _gameManager.players.Keys){
            var cardObjects = GameManager.CardInfosToGameObjects(player.hand);
            _cardCollectionPanel.TargetShowCardCollection(player.connectionToClient, 
                cardObjects, player.hand);
        }

        _cardCollectionPanel.RpcBeginDiscard();
    }
    public void PlayerSelectedDiscardCards(PlayerManager player) => PlayerIsReady(player);

    private void FinishDiscard() {
        foreach (var player in _gameManager.players.Keys) {
            player.DiscardSelection();
        }

        _cardCollectionPanel.RpcFinishDiscard();
        _handManager.RpcResetHighlight();
        
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #endregion

    #region Develop
    private void Develop(){
        _selectedKingdomCards = new Dictionary<PlayerManager, List<CardInfo>>();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginPhase(Phase.Develop);

        foreach (var player in _gameManager.players.Keys) {
            if (player.chosenPhases.Contains(Phase.Develop)){
                _kingdom.TargetDevelopBonus(player.connectionToClient, _gameManager.developPriceReduction);
            } 
        }
    }

    public void PlayerSelectedDevelopCard(PlayerManager player, List<CardInfo> cards)
    {
        var totalCost = 0;
        foreach(var card in cards){
            var cost = card.cost;
            if (player.chosenPhases.Contains(Phase.Develop)) cost -= _gameManager.developPriceReduction;
            totalCost += cost;
        }
        player.Cash -= totalCost;

        _selectedKingdomCards.Add(player, cards);
        PlayerIsReady(player);
    }

    private void DevelopSpawnAndReset()
    {
        foreach (var (owner, cards) in _selectedKingdomCards) {

            foreach (var cardInfo in cards) {
                _playerInterfaceManager.RpcLog("<color=#4f2d00>" + owner.PlayerName + " develops " + cardInfo.title + "</color>");
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
        _boardManager.ShowHolders(true);        
        _cardCollectionPanel.RpcBeginDeploy();
    }

    public void PlayerDeployedCard(PlayerManager player, GameObject card) {
        player.Deploys--;
        player.Cash -= card.GetComponent<CardStats>().cardInfo.cost;

        _selectedHandCards[player].Add(card);
        PlayerIsReady(player);        
    }

    public void PlayerSkipsDeploy(PlayerManager player) {
        player.Deploys--;
        PlayerIsReady(player);
    }

    private void DeployEntities(){
        var anotherDeploy = false;
        foreach(var (player, cards) in _selectedHandCards) {
            foreach (var card in cards) {
                var cardInfo = card.GetComponent<CardStats>().cardInfo;
                _gameManager.SpawnFieldEntity(player, card);
            }
            cards.Clear();
            if (player.Deploys > 0) anotherDeploy = true;
        }

        if(anotherDeploy) Deploy();
        else EndDeploy();
    }

    private void EndDeploy()
    {
        _cardCollectionPanel.RpcFinishDeploy();
        _handManager.RpcResetDeployability();
        PlayersStatsResetAndDiscardMoney();
        _boardManager.ShowHolders(false);
        
        UpdateTurnState(TurnState.NextPhase);
    }
    
    #endregion

    private void Combat() => combatManager.UpdateCombatState(CombatState.Attackers);
    public void CombatCleanUp() => UpdateTurnState(TurnState.NextPhase);
    
    #region Recruit

    private void Recruit(){
        _selectedKingdomCards = new Dictionary<PlayerManager, List<CardInfo>>();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginPhase(Phase.Recruit);

        foreach (var playerManager in _gameManager.players.Keys) {
            var nbRecruits = _gameManager.turnRecruits;
            playerManager.Recruits = nbRecruits;
        }
    }

    public void PlayerSelectedRecruitCard(PlayerManager player, CardInfo card)
    {
        player.Recruits--;
        player.Cash -= card.cost;
        if (_selectedKingdomCards.ContainsKey(player))
            _selectedKingdomCards[player].Add(card);
        else
            _selectedKingdomCards.Add(player, new List<CardInfo> { card });
        
        _kingdom.TargetResetRecruit(player.connectionToClient, player.Recruits);

        // Waiting for player to use remaining recruit actions
        if (player.Recruits > 0) return;
        
        PlayerIsReady(player);
    }

    private void RecruitSpawnAndReset()
    {
        foreach (var (owner, cards) in _selectedKingdomCards) {
            foreach (var cardInfo in cards) {
                _gameManager.SpawnCreature(owner, cardInfo);
                _playerInterfaceManager.RpcLog("<color=#4f2d00>" + owner.PlayerName + " recruits " + cardInfo.title + "</color>");
            }
        }
        
        PlayersStatsResetAndDiscardMoney();
        _kingdom.RpcEndRecruit();

        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion 

    #region Prevail
    private void Prevail(){
        foreach(var player in _gameManager.players.Keys){
            // Starts phase on clients individually with 2nd argument = true for the bonus
            _prevailPanel.TargetBeginPrevailPhase(player.connectionToClient, player.chosenPhases.Contains(Phase.Prevail));
        }
    }

    public void PlayerSelectedPrevailOptions(PlayerManager player, List<PrevailOption> options){
        _playerPrevailOptions[player] = options;
        PlayerIsReady(player);
    }

    private void StartPrevailOptions(){
        _readyPlayers.Clear();
        _prevailPanel.RpcOptionsSelected();

        // Tracking which options will be played
        foreach (var optionLists in _playerPrevailOptions.Values){
            foreach (var option in optionLists){
                if (_prevailOptionsToPlay.Contains(option)) continue;
                _prevailOptionsToPlay.Add(option);
            }
        }

        _prevailOptionsToPlay.Sort();
        NextPrevailOption();
    }

    private void NextPrevailOption(){

        // All options have been played
        if (_prevailOptionsToPlay.Count == 0){
            PrevailCleanUp();
            return;
        }

        var nextOption = _prevailOptionsToPlay[0];
        _prevailOptionsToPlay.RemoveAt(0);

        switch(nextOption){
            case PrevailOption.Trash:
                StartPrevailTrash();
                break;
            case PrevailOption.Score:
                print("<color=yellow> Prevail score not implemented yet </color>");
                NextPrevailOption();
                break;
            case PrevailOption.TopDeck:
                print("<color=yellow> Prevail top-deck not implemented yet </color>");
                NextPrevailOption();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StartPrevailTrash(){
        turnState = TurnState.Trash;
        foreach (var (player, options) in _playerPrevailOptions){
            var nbPicks = options.Count(option => option == PrevailOption.Trash);
            _cardCollectionPanel.TargetBeginTrash(player.connectionToClient, nbPicks);
        }
    }

    public void PlayerSelectedTrashCards(PlayerManager player, List<GameObject> selectedCards){
        _selectedHandCards[player] = selectedCards;
        PlayerIsReady(player);
    }

    public void PlayerSkipsTrash(PlayerManager player) => PlayerIsReady(player);

    private void FinishPrevailTrash(){

        var tempList = new List<GameObject>();
        foreach (var (player, cards) in _selectedHandCards){
            foreach (var card in cards){
                var stats = card.GetComponent<CardStats>();
                player.hand.Remove(stats.cardInfo);
                player.RpcTrashCard(card);
                tempList.Add(card);
            }
            player.trashSelection.Clear();
            cards.Clear();
        }

        foreach(var obj in tempList){
            // NetworkServer.Destroy(obj);
            obj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        }

        _prevailPanel.RpcReset();
        _cardCollectionPanel.RpcFinishTrash();
        _handManager.RpcResetHighlight();

        NextPrevailOption();
    }

    private void PrevailCleanUp(){
        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion

    private void CleanUp(){
        if(GameEnds()) return;

        PlayersStatsResetAndDiscardMoney(endOfTurn: true);       
        _readyPlayers.Clear();
        UpdateTurnState(TurnState.PhaseSelection);
    }

    private bool GameEnds(){
        var gameEnds = false;
        foreach (var (player, health)  in _playerHealth){
            if (health > 0) continue;
            
            OnPlayerDies?.Invoke(player);
            gameEnds = true;
        }

        if (gameEnds){
            _gameManager.EndGame();
            UpdateTurnState(TurnState.Idle);
        }

        return gameEnds;
    }
    
    #region HelperFunctions
    public void PlayerIsReady(PlayerManager player){
        if(!_readyPlayers.Contains(player)) _readyPlayers.Add(player);
        if (_readyPlayers.Count < _nbPlayers) return;
        
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
                DeployEntities();
                break;
            case TurnState.Recruit:
                RecruitSpawnAndReset();
                break;
            case TurnState.Prevail:
                StartPrevailOptions();
                break;
            case TurnState.Trash:
                FinishPrevailTrash();
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
    
    private void PlayersStatsResetAndDiscardMoney(bool endOfTurn = false)
    {
        _boardManager.DiscardMoney();
        _handManager.RpcHighlightMoney(false);

        foreach (var player in _gameManager.players.Keys)
        {
            player.DiscardMoneyCards();
            player.moneyCards.Clear();
            
            player.Cash = _gameManager.turnCash;
            player.Deploys = _gameManager.turnDeploys;
            player.Recruits = _gameManager.turnRecruits;

            if (!endOfTurn) continue;
            player.chosenPhases.Clear();
            player.chosenPrevailOptions.Clear();
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
    Trash,
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
