using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Cysharp.Threading.Tasks;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Game state")]
    public static TurnState TurnState { get; private set; }
    [SerializeField] private TurnState turnState;
    private GameOptions _gameOptions;
    private int _nbPlayers;

    [Header("Helper Fields")]
    [SerializeField] private List<TurnState> _phasesToPlay = new();
    [SerializeField] private List<int> _readyPlayers = new();
    [SerializeField] private List<int> _skippedPlayers = new();
    
    // Managers
    private GameManager _gameManager;
    private Market _market;
    private InteractionPanel _interactionPanel;
    private PrevailPanel _prevailPanel;
    private PlayerInterfaceManager _logger;
    private BoardManager _boardManager;
    private CombatManager _combatManager;
    [SerializeField] private AbilityQueue _abilityQueue;
    [SerializeField] private PhasePanel _phasePanel;

    // Other helpers
    private readonly Dictionary<PlayerManager, List<CardStats>> _selectedCards = new();
    private readonly Dictionary<PlayerManager, CardInfo> _selectedMarketCards = new();
    private readonly List<(int, CardType)> _boughtCards = new();
    private Dictionary<PlayerManager, TurnState[]> _playerPhaseChoices = new();
    private Dictionary<PlayerManager, List<PrevailOption>> _playerPrevailOptions = new();
    private readonly List<PrevailOption> _prevailOptionsToPlay = new();
    private readonly CardList _trashedCards = new(false, CardLocation.Trash);

    // Events
    public static event Action OnStartPhaseSelection;
    public static event Action<TurnState> OnTurnStateChanged;

    #region Setup
    private void Awake()
    {
        if (Instance == null) Instance = this;

        GameManager.OnGameStart += Prepare;
        PlayerManager.OnCashChanged += PlayerCashChanged;

        // Effects
        PriceReduction.OnMarketPriceReduction += PlayerGetsMarketBonus;
        Curse.OnPlayerGainsCurses += PlayerGainsCurses;

        _combatManager = GetComponent<CombatManager>();
    }

    private void Prepare(GameOptions gameOptions)
    {
        _gameOptions = gameOptions;
        _nbPlayers = gameOptions.SinglePlayer ? 1 : 2;

        SetupInstances(gameOptions);
        VariablesCaching(gameOptions);
        DrawInitialHand().Forget();
    }

    private void SetupInstances(GameOptions gameOptions)
    {
        _gameManager = GameManager.Instance;
        _boardManager = BoardManager.Instance;
        _market = Market.Instance;

        // Panels with setup (GameManager handles market setup)
        _logger = PlayerInterfaceManager.Instance;
        _logger.RpcPrepare(_gameManager.players.Values.ToArray(), gameOptions.NumberPhases);

        _interactionPanel = InteractionPanel.Instance;
        _interactionPanel.RpcPrepareInteractionPanel();

        _phasePanel.RpcPreparePhasePanel(gameOptions.NumberPhases);

        _prevailPanel = PrevailPanel.Instance;
        _prevailPanel.RpcPreparePrevailPanel();
    }

    private void VariablesCaching(GameOptions gameOptions)
    {
        var playerNames = new List<string>();
        foreach (var player in _gameManager.players.Values)
        {
            _playerPhaseChoices.Add(player, new TurnState[gameOptions.NumberPhases]);
            _playerPrevailOptions.Add(player, new List<PrevailOption>());
            _selectedCards.Add(player, new List<CardStats>());
            playerNames.Add(player.PlayerName);
        }

        // reverse order of _playerPhaseChoices to have host first
        _playerPhaseChoices = _playerPhaseChoices.Reverse().ToDictionary(x => x.Key, x => x.Value);
        _playerPrevailOptions = _playerPrevailOptions.Reverse().ToDictionary(x => x.Key, x => x.Value);
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
            .ContinueWith(() => OnStartPhaseSelection?.Invoke())
            .Forget();
    }

    public void PlayerSelectedPhases(PlayerManager player, TurnState[] phases)
    {
        _playerPhaseChoices[player] = phases;

        foreach (var phase in phases)
        {
            if (!_phasesToPlay.Contains(phase)) _phasesToPlay.Add(phase);
        }

        PlayerIsReady(player);
    }

    private void FinishPhaseSelection()
    {
        // Combat and Clean-Up each round
        _phasesToPlay.Add(TurnState.Attackers);
        _phasesToPlay.Add(TurnState.CleanUp);
        _phasesToPlay.Sort();

        _logger.RpcLog(_phasesToPlay);

        foreach (var (player, phases) in _playerPhaseChoices)
        {
            _phasePanel.RpcShowPhaseSelection(player, phases);
        }

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
            if (_playerPhaseChoices[player].Contains(turnState)) nbCardDraw += _gameOptions.extraDraw;

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

    public void PlayerSelectedDiscardCards(PlayerManager player, List<CardStats> selectedCards)
    {
        _selectedCards[player] = selectedCards;
        PlayerIsReady(player);
    }

    private void FinishDiscard()
    {
        foreach (var (player, cards) in _selectedCards)
        {
            print("Discarding on TurnManager: " + cards.Count);

            player.Cards.RemoveHandCards(cards, CardLocation.Discard);
            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Hand, CardLocation.Discard);
            _logger.RpcLog(player.ID, cards);
        }

        _interactionPanel.RpcResetPanel();
        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region Buy cards
    private void StartMarketPhase()
    {
        _boughtCards.Clear();
        
        var cardType = turnState == TurnState.Invent ? CardType.Technology : CardType.Creature;

        _market.RpcBeginMarketPhase(turnState);
        foreach (var player in _gameManager.players.Values)
        {
            // Each player gets +1 Buy
            player.Buys += _gameOptions.buys;

            // If player selected Invent or Recruit, they get the market bonus
            if (_playerPhaseChoices[player].Contains(turnState))
            {
                player.Buys += _gameOptions.extraBuys;
                PlayerGetsMarketBonus(player, cardType, _gameOptions.marketPriceReduction);
            }

            // Makes highlights appear
            _market.TargetCheckMarketPrices(player.connectionToClient, player.Cash);
        }

        // StartCoroutine(StartPhaseInteraction());
        StartPhaseInteraction();
    }

    private void PlayerGetsMarketBonus(PlayerManager player, CardType type, int amount)
    {
        _market.TargetMarketPriceReduction(player.connectionToClient, type, amount);
    }

    public void PlayerConfirmBuy(PlayerManager player, MarketSelection selection)
    {
        _boughtCards.Add((selection.index, selection.cardInfo.type));

        player.Buys--;
        player.Cash -= selection.cost;

        if (_selectedMarketCards.ContainsKey(player))
            _selectedMarketCards[player] = selection.cardInfo;
        else
            _selectedMarketCards.Add(player,  selection.cardInfo);

        _market.TargetResetMarket(player.connectionToClient, player.Buys);
        PlayerIsReady(player);
    }

    private void BuyCards()
    {
        foreach (var (owner, card) in _selectedMarketCards)
        {
            if (card.title == null) continue;

            PlayerGainsCard(owner, card);
        }

        _selectedMarketCards.Clear();
        _market.RpcMinButton();

        AsyncAwaitQueue(SorsTimings.showSpawnedCard)
            .ContinueWith(CheckBuyAnotherCard)
            .Forget();
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

    public void PlayerGainsCard(PlayerManager player, CardInfo cardInfo)
    {
        _gameManager.PlayerGainCard(player, cardInfo, CardLocation.Discard);
        _logger.RpcLog(player.ID, cardInfo.title, cardInfo.cost, LogType.Buy);
        AsyncAwaitQueue(SorsTimings.showSpawnedCard).Forget();
    }

    private void CheckBuyAnotherCard()
    {
        // Play another card if not all players have skipped
        if (AllPlayersSkipped()) {
            FinishBuyCard();
            return;
        }

        _market.RpcMaxButton();
        StartPhaseInteraction();
    }

    private void FinishBuyCard()
    {
        // Replace tiles that were bought by either player
        _market.EndMarketPhase(_boughtCards);
        PlayersDiscardMoney();
        _interactionPanel.RpcResetPanel();

        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region Play Cards

    private void StartPlayCard()
    {
        foreach(var player in _gameManager.players.Values) 
        {
            // Each player gets +1 Play
            player.Plays += _gameOptions.plays;

            // If player selected Develop or Deploy, they get bonus Plays
            if (_playerPhaseChoices[player].Contains(turnState)){
                player.Plays += _gameOptions.extraPlays;
                player.Cash += _gameOptions.extraCash;
            }
        }
        
        PlayCard();
    }

    private void PlayCard()
    {
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

    public void PlayerPlaysCard(PlayerManager player, CardStats card)
    {
        player.Plays--;
        player.Cash -= card.cardInfo.cost;

        _selectedCards[player].Add(card);
        PlayerIsReady(player);
    }

    private void PlayEntities()
    {
        Dictionary<GameObject, BattleZoneEntity> entities = new();
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards) {
                var cardInfo = card.cardInfo;
                entities.Add(card.gameObject, _gameManager.SpawnFieldEntity(player, cardInfo));
                _logger.RpcLog(player.ID, cardInfo.title, cardInfo.cost, LogType.Play);
            }
        }

        // Skip waiting for entity ability checks
        if (entities.Count == 0) CheckPlayAnotherCard();
        else AsyncPlayEntities(entities).ContinueWith(CheckPlayAnotherCard).Forget();
    }

    private void CheckPlayAnotherCard()
    {
        // Play another card if not all players have skipped
        if (AllPlayersSkipped()) FinishPlayCard();
        else PlayCard();
    }

    private void FinishPlayCard()
    {
        _interactionPanel.RpcResetPanel();
        _boardManager.ResetHolders();
        PlayersDiscardMoney();

        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion

    #region Prevail
    private void Prevail()
    {
        _prevailOptionsToPlay.Clear();
        foreach (var player in _gameManager.players.Values)
        {
            int nbOptions = _gameOptions.prevails;
            if (_playerPhaseChoices[player].Contains(turnState)) nbOptions += _gameOptions.extraPrevails;

            player.Prevails += nbOptions;
            _prevailPanel.TargetBeginPrevailPhase(player.connectionToClient, nbOptions);
        }
    }

    public void PlayerSelectedPrevailOptions(PlayerManager player, List<PrevailOption> options)
    {
        _playerPrevailOptions[player] = options;
        PlayerIsReady(player);
    }

    private void StartPrevailOptions()
    {
        _prevailPanel.RpcOptionsSelected();

        // Tracking which options will be played
        foreach (var optionLists in _playerPrevailOptions.Values)
        {
            foreach (var option in optionLists)
            {
                if (_prevailOptionsToPlay.Contains(option)) continue;
                _prevailOptionsToPlay.Add(option);
            }
        }

        _prevailOptionsToPlay.Sort();
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

    public void PlayerSelectedPrevailCards(PlayerManager player, List<CardStats> selectedCards)
    {
        _selectedCards[player] = selectedCards;
        PlayerIsReady(player);
    }

    private void FinishPrevailCardIntoHand()
    {
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards)
            {
                player.Cards.discard.Remove(card);
                player.Cards.hand.Add(card);
            }

            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Discard, CardLocation.Hand);
        }

        _interactionPanel.RpcResetPanel();
        NextPrevailOption();
    }

    private void FinishPrevailTrash()
    {
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards)
            {
                player.Cards.hand.Remove(card);

                _trashedCards.Add(card);
                card.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            }
            
            player.Cards.RpcMoveFromInteraction(cards, CardLocation.Hand, CardLocation.Trash);
        }

        _interactionPanel.RpcResetPanel();
        NextPrevailOption();
    }

    private void PrevailScoring(bool deducePoints = false)
    {
        foreach (var (player, options) in _playerPrevailOptions)
        {
            var nbPicks = options.Count(option => option == PrevailOption.Score);
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
        foreach (var player in _gameManager.players.Values)
            player.Cards.DrawCards(_gameOptions.cardDraw);

        await UniTask.Delay(SorsTimings.wait);

        await _abilityQueue.Resolve();
    }

    private async UniTask AsyncAwaitQueue(int delayMiliseconds)
    {
        await UniTask.Delay(delayMiliseconds);

        // Waiting for AbilityQueue to finish resolving Buy triggers
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

    private async UniTaskVoid PrevailCleanUp()
    {
        await UniTask.Delay(SorsTimings.wait);

        PrevailScoring(true);
        _playerPrevailOptions.Clear();
        _prevailPanel.RpcReset();

        UpdateTurnState(TurnState.NextPhase);
    }

    private async UniTaskVoid CleanUp()
    {
        // TODO: Should not use _market here but access it from _boardManager directly
        await _boardManager.BoardCleanUpEndOfTurn(_market.GetTileInfos());

        PlayersDiscardMoney();
        PlayersEmptyResources();

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
        // else if (newState == TurnState.PhaseSelection) PhaseSelection();
        else if (newState == TurnState.Draw) Draw();
        else if (newState == TurnState.Discard) Discard();
        else if (newState == TurnState.Invent || newState == TurnState.Recruit) StartMarketPhase();
        else if (newState == TurnState.Develop || newState == TurnState.Deploy) StartPlayCard();
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

    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        // Taking this "detour" to have server and clients in sync
        // Had troubles making Hand / Market listen to OnCashChanged directly..
        if(turnState == TurnState.Develop || turnState == TurnState.Deploy)
            _interactionPanel.TargetCheckPlayability(player.connectionToClient, turnState, newAmount);
        else if (turnState == TurnState.Invent || turnState == TurnState.Recruit)
            _market.TargetCheckMarketPrices(player.connectionToClient, newAmount);
    }

    private void PlayersDiscardMoney()
    {
        foreach (var player in _gameManager.players.Values)
        {
            // Returns unused money then discards the remaining cards
            player.Cards.DiscardMoneyCards();
            player.Cash = 0;
        }
    }

    private void PlayersEmptyResources()
    {
        foreach (var player in _gameManager.players.Values)
        {
            player.Buys = 0;
            player.Plays = 0;
            player.Prevails = 0;
        }
    }

    internal void PlayerClickedUndoButton(PlayerManager player)
    {
        _interactionPanel.TargetUndoMoneyPlay(player.connectionToClient);
    }

    private void StartPhaseInteraction(PrevailOption currentPrevailOption = PrevailOption.None)
    {
        foreach (var (player, selection) in _selectedCards)
        {
            // Always clear selection from last interaction
            selection.Clear();

            var nbInteractions = GetNumberOfInteractions(player, currentPrevailOption);
            var collection = GetCollection(player);

            print($" - {turnState}: phase interaction - {player.PlayerName} has {nbInteractions} options available");
            _interactionPanel.TargetStartInteraction(player.connectionToClient, collection, turnState, nbInteractions);
            player.Cash = player.Cash; // Trigger TargetCheckPlayability
        }
    }

    private int GetNumberOfInteractions(PlayerManager player, PrevailOption currentPrevailOption)
    {
        int numberInteractions = turnState switch
        {
            TurnState.Discard => _gameOptions.phaseDiscard,
            TurnState.Invent or TurnState.Recruit => player.Buys > 0 ? 1 : 0,
            TurnState.Develop or TurnState.Deploy => CheckNumberOfPossiblePlays(player),
            TurnState.CardSelection or TurnState.Trash => _playerPrevailOptions[player].Count(option => option == currentPrevailOption),
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
        _interactionPanel.RpcResetPanel();
        _market.RpcEndMarketPhase();

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
        PlayerManager.OnCashChanged -= PlayerCashChanged;
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