using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(CombatManager))]
public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Game state")]
    public TurnState turnState;
    private GameOptions _gameOptions;
    private int _nbPlayers;

    [Header("Helper Fields")]
    [SerializeField] private List<TurnState> _phasesToPlay = new();
    [SerializeField] private List<int> _readyPlayers = new();
    [SerializeField] private List<int> _skippedPlayers = new();
    
    // Managers
    private GameManager _gameManager;
    private NetworkObjectSpawner _networkObjectSpawner;
    private Market _market;
    private InteractionPanel _interactionPanel;
    private PrevailPanel _prevailPanel;
    private PlayerInterfaceManager _logger;
    private BoardManager _boardManager;
    private CombatManager _combatManager;
    [SerializeField] private AbilityQueue _abilityQueue;
    [SerializeField] private PhasePanel _phasePanel;

    // Other helpers
    private List<PrevailOption> _prevailOptionsToPlay = new();
    private readonly CardList _trashedCards = new(false, CardLocation.Trash);

    // Events
    public static event Action<TurnState> OnTurnStateChanged;

    #region Setup
    private void Awake()
    {
        if (Instance == null) Instance = this;

        GameManager.OnGameStart += Prepare;
        PriceReduction.OnMarketPriceReduction += PlayerGetsMarketBonus;
        Curse.OnPlayerGainsCurses += PlayerGainsCurses;

        _combatManager = GetComponent<CombatManager>();
    }

    private void Prepare(GameOptions gameOptions)
    {
        _gameOptions = gameOptions;
        _nbPlayers = gameOptions.SinglePlayer ? 1 : 2;

        SetupInstances(gameOptions);
        DrawInitialHand().Forget();
    }

    private void SetupInstances(GameOptions gameOptions)
    {
        _gameManager = GameManager.Instance;
        _boardManager = BoardManager.Instance;
        _logger = PlayerInterfaceManager.Instance;
        _prevailPanel = PrevailPanel.Instance;
        _interactionPanel = InteractionPanel.Instance;
        _market = Market.Instance;

        List<PlayerIdName> players = _gameManager.players.Values
            .Select(p => new PlayerIdName(p.ID, p.PlayerName))
            .ToList();
        _logger.RpcPrepare(players, gameOptions.NumberPhases);

        _interactionPanel.RpcPrepareInteractionPanel();
        _phasePanel.RpcPreparePhasePanel(gameOptions.NumberPhases);

        _networkObjectSpawner = ServiceLocator.Global.Get<NetworkObjectSpawner>();
    }

    #endregion

    #region Phase Selection
    private void PhaseSelection()
    {
        turnState = TurnState.PhaseSelection;
        _gameManager.turnNumber++;

        // Update UI
        _logger.RpcBeginTurn(_gameManager.turnNumber);
        OnTurnStateChanged?.Invoke(TurnState.PhaseSelection);

        // Wait for animation and abilities
        BeginningOfTurn()
            .ContinueWith(() => _interactionPanel.RpcStartPhaseSelection())
            .Forget();
    }

    private void FinishPhaseSelection()
    {
        foreach (var player in _gameManager.players.Values)
        {
            var chosenPhases = player.TurnContext.PhaseChoices;
            _phasesToPlay.AddRange(chosenPhases);
            _phasePanel.RpcShowPhaseSelection(player, chosenPhases);
        }

        // Combat and Clean-Up each round
        _phasesToPlay.Add(TurnState.Attackers);
        _phasesToPlay.Add(TurnState.CleanUp);
        _phasesToPlay = _phasesToPlay.Distinct().ToList();
        _phasesToPlay.Sort();

        _logger.RpcLog(_phasesToPlay);

        UpdateTurnState(TurnState.NextPhase);
    }

    public void NextPhase()
    {
        Enum.TryParse(_phasesToPlay[0].ToString(), out TurnState nextTurnState);
        _phasesToPlay.RemoveAt(0);

        // To update SM and Phase Panel
        OnTurnStateChanged?.Invoke(nextTurnState);

        AsyncAwaitQueue(SorsTimings.waitShort)
            .ContinueWith(() => {
                UpdateTurnState(nextTurnState);
                _logger.RpcLog(nextTurnState);
            })
            .Forget();
    }
    #endregion

    #region Drawing

    private void Draw()
    {
        foreach (var player in _gameManager.players.Values)
        {
            var nbCardDraw = _gameOptions.cardDraw;
            if (player.TurnContext.PhaseChoices.Contains(turnState)) nbCardDraw += _gameOptions.extraDraw;

            player.Cards.DrawCards(nbCardDraw);
            _logger.RpcLog(player.ID, nbCardDraw);
        }

        UpdateTurnState(TurnState.Discard);
    }

    private void Discard()
    {
        OnTurnStateChanged?.Invoke(TurnState.Discard);
        StartPhaseInteraction();
    }

    private void FinishDiscard()
    {
        // TODO: Move this (and all other _interactionPanel logic) to interaction panel
        // and current state? Will need player references and be from Server tho... 

        foreach (var player in _gameManager.players.Values)
        {
            var cards = _networkObjectSpawner.GetCardListByIds(player.TurnContext.SelectedCardIds);

            player.Cards.RemoveHandCards(cards, CardLocation.Discard);
            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Hand, CardLocation.Discard);
            _logger.RpcLog(player.ID, cards);
        }

        _interactionPanel.RpcFinishState();
        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region Buy cards
    private void StartBuyPhase()
    {
        _market.RpcBeginMarketPhase(turnState);

        foreach (var player in _gameManager.players.Values)
        {
            // Each player gets +1 Buy
            player.Buys += _gameOptions.buys;

            // If player selected Invent or Recruit, they get the market bonus
            if (player.TurnContext.PhaseChoices.Contains(turnState))
            {
                player.Buys += _gameOptions.extraBuys;
                PlayerGetsMarketBonus(player, _gameOptions.marketPriceReduction);
            }

            // Makes highlights appear
            _market.TargetCheckMarketPrices(player.connectionToClient, player.Cash);
        }

        StartPhaseInteraction();
    }

    private void PlayerGetsMarketBonus(PlayerManager player, int amount, CardType type = CardType.None)
    {
        var cardType = turnState == TurnState.Invent ? CardType.Technology : CardType.Creature;
        _market.TargetMarketPriceReduction(player.connectionToClient, cardType, amount);
    }

    public void PlayerConfirmBuy(PlayerManager player, int marketIndex)
    {
        _market.PlayerConfirmsChoice(marketIndex);
        _market.TargetResetMarket(player.connectionToClient);

        PlayerIsReady(player);
    }

    private void BuyCards()
    {
        foreach (var player in _gameManager.players.Values)
        {
            var cardInfo = player.TurnContext.SelectedCard;
            if (! cardInfo.HasValue) continue;

            PlayerGainsCard(player, cardInfo.Value);

            player.Cards.DiscardMoneyCards(player.ID);
        }
        _market.RpcMinButton();

        AsyncAwaitQueue(SorsTimings.showSpawnedCard)
            .ContinueWith(RestockMarket)
            .Forget();
    }

    public void PlayerGainsCard(PlayerManager player, CardInfo cardInfo)
    {
        player.Buys--;
        player.Cash = player.TurnContext.CashBuffer;

        _gameManager.PlayerGainCard(player, cardInfo, CardLocation.Discard);
        _logger.RpcLog(player.ID, cardInfo.title, cardInfo.cost, LogType.Buy);
        AsyncAwaitQueue(SorsTimings.showSpawnedCard).Forget();
    }

    private void PlayerGainsCurses(PlayerManager player, int amount)
    {
        // TODO: In singleplayer, player is null
        for(int i=0; i<amount; i++)
        {
            _gameManager.PlayerGainCurse(player);
            _logger.RpcLog(player.ID, "gains a curse");
        }

        AsyncAwaitQueue(SorsTimings.showSpawnedCard + SorsTimings.waitShort).Forget();
    }

    private void RestockMarket()
    {
        // Replace tiles that were bought by either player
        _market.ReplaceTiles(turnState);
        CheckBuyAnotherCard();
    }

    private void CheckBuyAnotherCard()
    {
        // Play another card if not all players have skipped
        if (AllPlayersSkipped()) {
            FinishBuyCard();
            return;
        }

        _market.RpcMaxButton();
        foreach (var player in _gameManager.players.Values)
            _market.TargetCheckMarketPrices(player.connectionToClient, player.Cash);
        StartPhaseInteraction();
    }

    private void FinishBuyCard()
    {
        foreach (var player in _gameManager.players.Values)
            player.Cash = 0;

        _market.RpcEndMarketPhase();
        
        _interactionPanel.RpcFinishState();
        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region Play Cards

    private void StartPlayPhase()
    {
        foreach(var player in _gameManager.players.Values) 
        {
            // Each player gets +1 Play
            player.Plays += _gameOptions.plays;

            // BONUS: If player selected Develop or Deploy
            if (player.TurnContext.PhaseChoices.Contains(turnState))
            {
                player.Plays += _gameOptions.extraPlays;
                player.Cash += _gameOptions.extraCash;
            }
        }

        StartPhaseInteraction();
    }

    private int CheckNumberOfPossiblePlays(PlayerManager player)
    {
        int numberPlays = player.Plays > 0 ? 1 : 0;
        int numberSlots = _boardManager.CheckNumberOfFreeSlots(player.isLocalPlayer, turnState);
        print($" - {turnState}: {player.PlayerName} has {numberSlots} free slots");

        if (numberSlots == -1) throw new Exception("Trying to access entity holders in invalid phase: " + turnState);

        return Math.Min(numberPlays, numberSlots);
    }

    public void PlayerConfirmPlay(PlayerManager player)
    {       
        PlayerIsReady(player);
    }

    private void PlayEntities()
    {
        Dictionary<GameObject, BattleZoneEntity> entities = new();
        foreach (var player in _gameManager.players.Values)
        {
            var cardInfo = player.TurnContext.SelectedCard;
            if (! cardInfo.HasValue) continue;

            player.Plays--;
            player.Cash = player.TurnContext.CashBuffer;

            var card = _networkObjectSpawner.GetCardById(cardInfo.Value.goID);
            player.Cards.RemoveHandCards(new List<CardStats> { card }, CardLocation.PlayZone);

            entities.Add(card.gameObject, _gameManager.SpawnFieldEntity(player, card.cardInfo));
            PlayerPlaysCard(player, card.cardInfo);

            player.Cards.DiscardMoneyCards(player.ID);
        }

        // Skip waiting for entity ability checks
        if (entities.Count == 0) CheckPlayAnotherCard();

        // TODO: Transform this to make one at a time enter? Would be clearer for players
        // and make ETB triggers clearer. Compare to PlayerGainsCard.
        else AsyncPlayEntities(entities).ContinueWith(CheckPlayAnotherCard).Forget();
    }

    private void PlayerPlaysCard(PlayerManager player, CardInfo cardInfo) 
    {
        _logger.RpcLog(player.ID, cardInfo.title, cardInfo.cost, LogType.Play);
    }

    private void CheckPlayAnotherCard()
    {
        // Play another card if not all players have skipped
        if (AllPlayersSkipped()) FinishPlayCard();
        else StartPhaseInteraction();
    }

    private void FinishPlayCard()
    {
        _boardManager.ResetHolders();
        foreach (var player in _gameManager.players.Values)
        {
            player.Cash = 0;
        }

        _interactionPanel.RpcFinishState();
        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion

    #region Prevail
    private void Prevail()
    {
        _prevailOptionsToPlay.Clear();
        foreach (var player in _gameManager.players.Values)
        {
            player.TurnContext.PrevailOptions.Clear();

            int nbOptions = _gameOptions.prevails;
            if (player.TurnContext.PhaseChoices.Contains(turnState)) nbOptions += _gameOptions.extraPrevails;

            player.Prevails += nbOptions;
            _prevailPanel.TargetBeginPrevailPhase(player.connectionToClient, nbOptions);
        }
    }

    private void StartPrevailOptions()
    {
        _prevailPanel.RpcOptionsSelected();

        _prevailOptionsToPlay = _gameManager.players.Values
            .SelectMany(p => p.TurnContext.PrevailOptions)
            .Distinct()
            .OrderBy(o => o)
            .ToList();
        
        NextPrevailOption();
    }
    private void NextPrevailOption()
    {
        _readyPlayers.Clear();
        if (_prevailOptionsToPlay.Count == 0)
        {
            PrevailCleanUp().Forget();
            return;
        }

        var nextOption = _prevailOptionsToPlay[0];
        _prevailOptionsToPlay.RemoveAt(0);

        Enum.TryParse(nextOption.ToString(), out TurnState nextTurnState);
        turnState = nextTurnState;

        OnTurnStateChanged?.Invoke(nextTurnState);

        if (nextOption == PrevailOption.Score) PrevailScoring();
        else StartPhaseInteraction(nextOption);
    }

    private void FinishPrevailCardIntoHand()
    {
        foreach (var player in _gameManager.players.Values)
        {
            if(player.TurnContext.SelectedCardIds.Count() == 0) continue;

            var cards = _networkObjectSpawner.GetCardListByIds(player.TurnContext.SelectedCardIds);
            foreach (var card in cards)
            {
                player.Cards.discard.Remove(card);
                player.Cards.hand.Add(card);
            }

            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Discard, CardLocation.Hand);
        }

        _interactionPanel.RpcFinishState();
        FinishPrevailOption(SorsTimings.wait).Forget();
    }

    private void FinishPrevailTrash()
    {
        foreach (var player in _gameManager.players.Values)
        {
            if(player.TurnContext.SelectedCardIds.Count() == 0) continue;
            var cards = _networkObjectSpawner.GetCardListByIds(player.TurnContext.SelectedCardIds);
            foreach (var card in cards)
            {
                player.Cards.hand.Remove(card);

                _trashedCards.Add(card);
                card.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            }
            
            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Hand, CardLocation.Trash);
        }

        _interactionPanel.RpcFinishState();

        NextPrevailOption();
    }

    private void PrevailScoring(bool deducePoints = false)
    {
        foreach (var player in _gameManager.players.Values)
        {
            var nbPicks = player.TurnContext.PrevailOptions.Count(option => option == PrevailOption.Score);
            if (deducePoints) player.Score -= nbPicks;
            else player.Score += nbPicks;
        }

        if (deducePoints) return;
        NextPrevailOption();
    }
    #endregion    

    #region Async Functions

    private async UniTaskVoid DrawInitialHand()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_gameOptions.SkipCardSpawnAnimations ? 0.5f : 4f));

        // StateFile is NOT null or empty if we load from a file eg. state.json
        // Dont want ETB triggers for entities from game state and only draw initial hand in normal game start 
        if(! string.IsNullOrEmpty(_gameOptions.StateFile)) _abilityQueue.ClearQueue();
        else {
            foreach(var player in _gameManager.players.Values) {
                _logger.RpcLog(player.ID);
                player.Cards.deck.Shuffle();
                player.Cards.DrawCards(_gameOptions.InitialHandSize);
            }
            await UniTask.Delay(SorsTimings.wait);
        }

        if(_gameOptions.SaveStates) _boardManager.PrepareGameStateFile(_market.GetTileInfos());
        PhaseSelection();
    }

    private async UniTask BeginningOfTurn()
    {
        // Reset players and draw per turn
        foreach (var player in _gameManager.players.Values) {
            player.TurnContext.PhaseChoices.Clear();
            player.Cards.DrawCards(_gameOptions.cardDraw);
            _logger.RpcLog(player.ID, _gameOptions.cardDraw);
        }

        await UniTask.Delay(SorsTimings.wait);

        await _abilityQueue.Resolve();
    }

    private async UniTask AsyncAwaitQueue(int delayMiliseconds)
    {
        await UniTask.Delay(delayMiliseconds);

        // Waiting for AbilityQueue to finish resolving triggers (phase transition, buy, gain)
        await _abilityQueue.Resolve();
    }

    private async UniTask AsyncPlayEntities(Dictionary<GameObject, BattleZoneEntity> entities)
    {
        // Keeps track of card <-> entity relation
        await _boardManager.PlayEntities(entities);

        // Waiting for AbilityQueue to finish resolving ETB triggers
        await _abilityQueue.Resolve();
    }

    public async UniTaskVoid CombatCleanUp()
    {
        await UniTask.Delay(SorsTimings.waitLong);
        UpdateTurnState(TurnState.NextPhase);
    }

    private async UniTaskVoid FinishPrevailOption(int delayMiliseconds)
    {
        await UniTask.Delay(delayMiliseconds);
        NextPrevailOption();
    }

    private async UniTaskVoid PrevailCleanUp()
    {
        await UniTask.Delay(SorsTimings.wait);

        PrevailScoring(true);
        _prevailPanel.RpcReset();

        UpdateTurnState(TurnState.NextPhase);
    }

    private async UniTaskVoid CleanUp()
    {
        // TODO: Should not use _market here but access it from _boardManager directly
        await _boardManager.BoardCleanUpEndOfTurn(_market.GetTileInfos());

        ResetPlayers();

        await UniTask.Delay(SorsTimings.waitLong);
    
        PhaseSelection();
    }

    #endregion

    #region Helpers

    private void UpdateTurnState(TurnState newState)
    {
        turnState = newState;

        if(GameEnds()){
            _gameManager.EndGame();
            newState = TurnState.Idle;
        }

        _readyPlayers.Clear();
        _skippedPlayers.Clear();

        if (newState == TurnState.NextPhase) NextPhase();
        else if (newState == TurnState.Draw) Draw();
        else if (newState == TurnState.Discard) Discard();
        else if (newState == TurnState.Invent || newState == TurnState.Recruit) StartBuyPhase();
        else if (newState == TurnState.Develop || newState == TurnState.Deploy) StartPlayPhase();
        else if (newState == TurnState.Prevail) Prevail();
        else if (newState == TurnState.CleanUp) CleanUp().Forget();
        else if (newState == TurnState.Attackers) _combatManager.UpdateCombatState(TurnState.Attackers);
        else if (newState == TurnState.Idle) {}
        else throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
    }

    public void PlayerIsReady(PlayerManager player)
    {
        if (!_readyPlayers.Contains(player.ID)) _readyPlayers.Add(player.ID);
        // OnPlayerIsReady?.Invoke(player.ID, turnState);
        print($"    - {player.PlayerName} ready ({_readyPlayers.Count} / {_nbPlayers})");

        // All players are ready
        if (_readyPlayers.Count < _nbPlayers) return;
        _readyPlayers.Clear();

        if (turnState == TurnState.PhaseSelection) FinishPhaseSelection();
        else if (turnState == TurnState.Discard) FinishDiscard();
        else if (turnState == TurnState.Invent || turnState == TurnState.Recruit) BuyCards();
        else if (turnState == TurnState.Develop || turnState == TurnState.Deploy) PlayEntities();
        else if (turnState == TurnState.Prevail) StartPrevailOptions();
        else if (turnState == TurnState.CardSelection) FinishPrevailCardIntoHand();
        else if (turnState == TurnState.Trash) FinishPrevailTrash();
        else throw new ArgumentOutOfRangeException(nameof(turnState), turnState, null);
    }

    public void PlayerSkipsInteraction(PlayerManager player)
    {
        _skippedPlayers.Add(player.ID);
        PlayerIsReady(player);
    }

    private bool AllPlayersSkipped()
    {
        // Add each player that skipped to _readyPlayers

        foreach (var player in _gameManager.players.Values) 
        {
            if (_skippedPlayers.Contains(player.ID)) _readyPlayers.Add(player.ID);
            else if ((turnState == TurnState.Invent || turnState == TurnState.Recruit) && player.Buys <= 0) _readyPlayers.Add(player.ID);
            else if ((turnState == TurnState.Develop || turnState == TurnState.Deploy) && player.Plays <= 0) _readyPlayers.Add(player.ID);
        }

        return _readyPlayers.Count == _nbPlayers;
    }

    private void ResetPlayers()
    {
        foreach (var player in _gameManager.players.Values)
        {
            player.Buys = 0;
            player.Plays = 0;
            player.Prevails = 0;
            player.TurnContext.Reset();
        }
    }

    private void StartPhaseInteraction(PrevailOption currentPrevailOption = PrevailOption.None)
    {
        foreach (var player in _gameManager.players.Values)
        {
            // Reset selection from last interaction
            player.TurnContext.ResetState();

            var nbInteractions = GetNumberOfInteractions(player, currentPrevailOption);
            var collection = GetCollection(player);

            print($" - {turnState}: {player.PlayerName} has {nbInteractions} interactions");
            _interactionPanel.TargetStartCardInteraction(player.connectionToClient, collection, turnState, nbInteractions);
        }
    }

    private int GetNumberOfInteractions(PlayerManager player, PrevailOption currentPrevailOption)
    {
        int numberInteractions = turnState switch
        {
            TurnState.Discard => _gameOptions.phaseDiscard,
            TurnState.Invent or TurnState.Recruit => player.Buys > 0 ? 1 : 0,
            TurnState.Develop or TurnState.Deploy => CheckNumberOfPossiblePlays(player),
            TurnState.CardSelection or TurnState.Trash => player.TurnContext.PrevailOptions.Count(option => option == currentPrevailOption),
            _ => -1
        };

        return numberInteractions;
    }

    // TODO: Could add interactions with other collections here 
    // Eg. opponent hand, trash, etc.
    private CardList GetCollection(PlayerManager player)
    {
        var collection = turnState switch
        {
            TurnState.CardSelection => player.Cards.discard,
            // TurnState.GetFromTrash => _trashedCards.Values.ToList(),
            _ => player.Cards.hand
        };

        return collection;
    }

    public CardList GetTrashedCards() => _trashedCards;

    public void ForceEndTurn()
    { // experimental
        _abilityQueue.ClearQueue();

        _boardManager.ResetHolders();
        _combatManager.CombatCleanUp(true);

        _prevailPanel.RpcOptionsSelected();
        _prevailPanel.RpcReset();
        _interactionPanel.RpcFinishState();
        _market.RpcEndMarketPhase();

        ResetPlayers();

        CleanUp().Forget();
    }

    public PlayerManager GetOpponentPlayer(PlayerManager player)
    {
        var players =  _gameManager.players.Values.ToArray();
        if (_gameOptions.SinglePlayer){
            players = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        }
        PlayerManager opponent = null;
        foreach (var p in players){
            if (p == player) continue;
            opponent = p;
            break;
        }
        return opponent;
    }

    private bool GameEnds()
    {
        var gameEnds = false;
        foreach (var player in _gameManager.players.Values)
        {
            if (player.Health > 0 && player.Score < _gameOptions.winScore) continue;
            if (player.Health <= 0) _gameManager.PlayerIsDead(player);
            if (player.Score >= _gameOptions.winScore) _gameManager.PlayerHasWinScore(player);
            gameEnds = true;
        }

        return gameEnds;
    }
    #endregion

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
        // PlayerManager.OnCashChanged -= PlayerCashChanged;
        PriceReduction.OnMarketPriceReduction -= PlayerGetsMarketBonus;
        Curse.OnPlayerGainsCurses -= PlayerGainsCurses;
    }
}

public enum TurnState : byte
{
    Idle,
    NextPhase,
    PhaseSelection,
    Draw,
    Discard,
    Invent,
    Develop,
    Attackers,
    Blockers,
    CombatDamage,
    CombatCleanUp,
    Recruit,
    Deploy,
    Prevail,
    CardSelection,
    Trash,
    CleanUp,
    None,
}