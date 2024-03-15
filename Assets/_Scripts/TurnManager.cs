using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using SorsGameState;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Entities")]
    private GameManager _gameManager;
    private Market _market;
    private HandInteractionPanel _cardCollectionPanel;
    private PhasePanel _phasePanel;
    private PrevailPanel _prevailPanel;
    private PlayerInterfaceManager _logger;
    private Hand _handManager;
    private BoardManager _boardManager;
    private CardEffectsHandler _cardEffectsHandler;
    [SerializeField] private CombatManager combatManager;

    [field: Header("Game state")]
    [SerializeField] private TurnState turnState;
    public static TurnState GetTurnState() => Instance.turnState;
    public List<Phase> phasesToPlay;
    private Dictionary<PlayerManager, Phase[]> _playerPhaseChoices = new();
    private List<PrevailOption> _prevailOptionsToPlay = new();
    private Dictionary<PlayerManager, List<PrevailOption>> _playerPrevailOptions = new();
    private List<PlayerManager> _readyPlayers = new();
    private List<PlayerManager> _skippedPlayers = new();
    private int _nbPlayers;
    private bool _skipCardDrawAnimations;

    [Header("Helper Fields")]
    private GameOptions _gameOptions;
    private Dictionary<PlayerManager, CardInfo> _selectedMarketCards = new();
    private Dictionary<PlayerManager, List<GameObject>> _selectedCards = new();

    // Events
    public static event Action<TurnState> OnPhaseChanged;
    public static event Action<PlayerManager> OnPlayerDies;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        GameManager.OnGameStart += Prepare;
    }

    private void UpdateTurnState(TurnState newState)
    {
        turnState = newState;
        _skippedPlayers.Clear();

        switch (turnState)
        {
            // --- Preparation and transition ---
            case TurnState.PhaseSelection:
                PhaseSelection();
                break;
            case TurnState.NextPhase:
                NextPhase();
                break;
            // --- Phases ---
            case TurnState.Draw:
                Draw();
                break;
            case TurnState.Discard:
                Discard();
                break;
            case TurnState.Invent:
                StartMarketPhase();
                break;
            case TurnState.Develop:
                StartPlayCard();
                break;
            case TurnState.Combat:
                Combat();
                break;
            case TurnState.Recruit:
                StartMarketPhase();
                break;
            case TurnState.Deploy:
                StartPlayCard();
                break;
            case TurnState.Prevail:
                Prevail();
                break;
            // --- Win check and turn reset ---
            case TurnState.CleanUp:
                CleanUp();
                break;
            case TurnState.Idle:
                _logger.RpcLog("Game finished", LogType.Standard);
                break;

            default:
                print("<color=red>Invalid turn state</color>");
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        _phasePanel.RpcChangeActionDescriptionText(turnState);
    }

    private void Prepare(GameOptions gameOptions)
    {
        _nbPlayers = gameOptions.SinglePlayer ? 1 : 2;
        _skipCardDrawAnimations = gameOptions.SkipCardSpawnAnimations;
        _gameOptions = gameOptions;

        SetupInstances(gameOptions);
        VariablesCaching(gameOptions);
        StartCoroutine(DrawInitialHand());
    }

    private void SetupInstances(GameOptions gameOptions)
    {
        _gameManager = GameManager.Instance;
        _handManager = Hand.Instance;
        _boardManager = BoardManager.Instance;
        _cardEffectsHandler = CardEffectsHandler.Instance;
        _market = Market.Instance;
        _logger = PlayerInterfaceManager.Instance;

        // Panels with setup (GameManager handles market setup)
        _cardCollectionPanel = HandInteractionPanel.Instance;
        _cardCollectionPanel.RpcPrepareCardCollectionPanel(gameOptions.phaseDiscard);
        _phasePanel = PhasePanel.Instance;
        _phasePanel.RpcPreparePhasePanel(gameOptions.NumberPhases);
        _prevailPanel = PrevailPanel.Instance;
        _prevailPanel.RpcPreparePrevailPanel();
    }

    private void VariablesCaching(GameOptions gameOptions)
    {
        var msg = "";
        foreach (var player in _gameManager.players.Keys)
        {
            _playerPhaseChoices.Add(player, new Phase[gameOptions.NumberPhases]);
            _playerPrevailOptions.Add(player, new List<PrevailOption>());
            _selectedCards.Add(player, new List<GameObject>());

            if (string.IsNullOrWhiteSpace(msg)) msg += $"{player.PlayerName}";
            else msg += $"vs. {player.PlayerName}";
        }
        if (_gameManager.isSinglePlayer) msg += "vs. Computer";
        _logger.RpcLog(msg, LogType.Standard);

        // reverse order of _playerPhaseChoices to have host first
        _playerPhaseChoices = _playerPhaseChoices.Reverse().ToDictionary(x => x.Key, x => x.Value);
        _playerPrevailOptions = _playerPrevailOptions.Reverse().ToDictionary(x => x.Key, x => x.Value);
        PlayerManager.OnCashChanged += PlayerCashChanged;
    }

    private IEnumerator DrawInitialHand()
    {
        float waitForSpawn = _skipCardDrawAnimations ? 0.5f : 4f;
        yield return new WaitForSeconds(waitForSpawn);

        // StateFile is NOT null or empty if we load from a file eg. state.json
        // Dont want ETB triggers for entities from game state and only draw initial hand in normal game start 
        if(! string.IsNullOrEmpty(_gameOptions.StateFile)) _cardEffectsHandler.ClearAbilitiesQueue();
        else {
            foreach(var player in _gameManager.players.Keys) {
                player.deck.Shuffle();
                player.DrawInitialHand(_gameOptions.InitialHandSize);
            }
            yield return new WaitForSeconds(SorsTimings.wait);
        }

        if(_gameOptions.SaveStates) _boardManager.PrepareGameStateFile(_market.GetTileInfos());        
        UpdateTurnState(TurnState.PhaseSelection);
    }

    #region PhaseSelection
    private void PhaseSelection()
    {
        _gameManager.turnNumber++;
        _logger.RpcLog($" ------------ Turn {_gameManager.turnNumber} ------------ ", LogType.TurnChange);
        // For phase panel
        OnPhaseChanged?.Invoke(TurnState.PhaseSelection);

        // Reset and draw per turn
        foreach (var player in _gameManager.players.Keys) {
            player.Buys = _gameOptions.turnBuys;
            player.Plays = _gameOptions.turnPlays;
            player.Prevails = _gameOptions.turnPrevails;

            player.chosenPhases.Clear();
            player.chosenPrevailOptions.Clear();

            player.DrawCards(_gameOptions.turnCardDraw);
        }

        _phasePanel.RpcBeginPhaseSelection(_gameManager.turnNumber);
    }

    public void PlayerSelectedPhases(PlayerManager player, Phase[] phases)
    {
        _playerPhaseChoices[player] = phases;

        foreach (var phase in phases)
        {
            if (!phasesToPlay.Contains(phase)) phasesToPlay.Add(phase);
        }

        PlayerIsReady(player);
    }

    private void FinishPhaseSelection()
    {
        // Combat each round
        phasesToPlay.Add(Phase.Combat);
        phasesToPlay.Sort();

        var msg = $"Phases to play:\n";
        for (int i = 0; i < phasesToPlay.Count; i++) msg += $"- {phasesToPlay[i]}\n";
        _logger.RpcLog(msg, LogType.Phase);

        foreach (var (player, phases) in _playerPhaseChoices)
        {
            player.RpcShowOpponentChoices(phases);
        }

        UpdateTurnState(TurnState.NextPhase);
    }

    private void NextPhase()
    {
        if (phasesToPlay.Count == 0)
        {
            UpdateTurnState(TurnState.CleanUp);
            return;
        }

        _readyPlayers.Clear();
        var nextPhase = phasesToPlay[0];
        phasesToPlay.RemoveAt(0);

        StartCoroutine(CheckNextPhaseTriggers(nextPhase));
    }

    public IEnumerator CheckNextPhaseTriggers(Phase nextPhase)
    {
        // To update SM and Phase Panel
        Enum.TryParse(nextPhase.ToString(), out TurnState nextTurnState);
        _logger.RpcLog($"------- {nextTurnState} -------", LogType.Phase);

        _cardEffectsHandler.CheckPhaseTriggers(nextPhase);
        while(_cardEffectsHandler.QueueResolving) yield return new WaitForSeconds(0.1f);

        OnPhaseChanged?.Invoke(nextTurnState);
        UpdateTurnState(nextTurnState);
    }

    #endregion

    #region Drawing

    private void Draw()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            var nbCardDraw = _gameOptions.turnCardDraw;
            if (player.chosenPhases.Contains(Phase.Draw)) nbCardDraw += _gameOptions.extraDraw;

            player.DrawCards(nbCardDraw);
            _logger.RpcLog($"{player.PlayerName} draws {nbCardDraw} cards", LogType.Standard);
        }

        UpdateTurnState(TurnState.Discard);
    }

    private void Discard()
    {
        ShowCardCollection();
    }
    public void PlayerSelectedDiscardCards(PlayerManager player) => PlayerIsReady(player);

    private void FinishDiscard()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            player.DiscardSelection();
        }

        _cardCollectionPanel.RpcResetPanel();
        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region Market phases (Invent, Recruit)
    private void StartMarketPhase()
    {
        // convert current turnState (TurnState enum) to Phase enum
        var phase = Phase.Recruit;
        var cardType = CardType.Creature;
        if (turnState == TurnState.Invent){
            phase = Phase.Invent;
            cardType = CardType.Technology;
        } 

        _handManager.RpcHighlightMoney(true);
        _market.RpcBeginPhase(phase);

        foreach (var player in _gameManager.players.Keys)
        {
            // Each player gets +1 Buy
            player.Buys++;

            // If player selected Invent or Recruit, they get the market bonus
            if (player.chosenPhases.Contains(phase))
            {
                player.Buys++;
                PlayerGetsMarketBonus(player, cardType, _gameOptions.marketPriceReduction);

                _market.TargetCheckMarketPrices(player.connectionToClient, player.Cash);
            }
        }
    }

    public void PlayerGetsMarketBonus(PlayerManager player, CardType type, int amount)
    {
        // Public because EffectHandler needs to call this
        _market.TargetMarketPriceReduction(player.connectionToClient, type, amount);
    }

    public void PlayerConfirmBuy(PlayerManager player, CardInfo card, int cost)
    {
        player.Buys--;
        player.Cash -= cost;

        if (_selectedMarketCards.ContainsKey(player))
            _selectedMarketCards[player] = card;
        else
            _selectedMarketCards.Add(player,  card);

        _market.TargetResetMarket(player.connectionToClient, player.Buys);
        PlayerIsReady(player);
    }

    public void PlayerSkipsBuy(PlayerManager player)
    {
        _skippedPlayers.Add(player);
        PlayerIsReady(player);
    }

    private void BuyCards()
    {
        foreach (var (owner, card) in _selectedMarketCards)
        {
            if (card.title == null) continue;

            print($"{owner.PlayerName} buys '{card.title}'");
            _logger.RpcLog($"{owner.PlayerName} buys '{card.title}'", LogType.CreatureBuy);
            _gameManager.PlayerGainCard(owner, card);
        }

        _selectedMarketCards.Clear();
        StartCoroutine(BuyCardsIntermission());
    }

    private IEnumerator BuyCardsIntermission()
    {
        _market.RpcMinButton();
        // Waiting for Entities abilities (ETB) being tracked 
        yield return new WaitForSeconds(SorsTimings.wait);

        // Waiting for CEH to set QueueResolving to false
        StartCoroutine(_cardEffectsHandler.StartResolvingQueue());
        while (_cardEffectsHandler.QueueResolving) 
            yield return new WaitForSeconds(0.1f);

        CheckBuyAnotherCard();
    }

    private void CheckBuyAnotherCard()
    {
        // Reset abilities and dead entities
        _boardManager.BoardCleanUp(_market.GetTileInfos(), false);

        // Add each player that skipped to _readyPlayers
        foreach (var player in _gameManager.players.Keys) 
        {
            if (_skippedPlayers.Contains(player)) _readyPlayers.Add(player);
            else if (player.Buys == 0) _readyPlayers.Add(player);
        }

        // Play another card if not all players have skipped
        if (_readyPlayers.Count != _gameManager.players.Count) {
            _market.RpcMaxButton();
            // Replace tile with intention to give more variation and a race to strong creatures
            // TODO: Check if I should do this for technologies as well
            // TODO: How to replace tiles for both players?
            // TODO: Or only do this in FinishBuyCard?
            // _market.RpcReplaceRecruitTile(card.title, nextTile);
        } else {
            FinishBuyCard();
        }
    }

    private void FinishBuyCard()
    {
        PlayersStatsResetAndDiscardMoney();
        _market.RpcEndMarketPhase();

        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region PlayCardPhase (Develop, Deploy)

    private void StartPlayCard()
    {
        // convert current turnState (TurnState enum) to Phase enum
        var phase = Phase.Develop;
        if (turnState == TurnState.Deploy) phase = Phase.Deploy;

        _boardManager.ShowHolders(true);
        foreach(var player in _gameManager.players.Keys) 
        {
            // Each player gets +1 Play
            player.Plays++;

            // If player selected Develop or Deploy, they get bonus Plays
            if (player.chosenPhases.Contains(phase)){
                player.Plays += _gameOptions.extraPlays;
                player.Cash += _gameOptions.extraCash;
            }
        }
        
        PlayCard();
    }

    private void PlayCard()
    {
        foreach (var cardsList in _selectedCards.Values) cardsList.Clear();

        _handManager.RpcHighlightMoney(true);
        ShowCardCollection();
    }

    public void PlayerPlaysCard(PlayerManager player, GameObject card)
    {
        player.Plays--;
        player.Cash -= card.GetComponent<CardStats>().cardInfo.cost;

        _selectedCards[player].Add(card);
        PlayerIsReady(player);
    }

    public void PlayerSkipsCardPlay(PlayerManager player)
    {
        _skippedPlayers.Add(player);
        PlayerIsReady(player);
    }

    private void PlayEntities()
    {
        Dictionary<GameObject, BattleZoneEntity> entities = new();
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards) entities.Add(card, _gameManager.SpawnFieldEntity(player, card));
        }

        // Keeps track of card <-> entity relation
        _boardManager.PlayEntities(entities);

        // Skip waiting for entity ability checks
        if (entities.Count == 0){
            CheckPlayAnotherCard();
            return;
        }
        
        StartCoroutine(PlayCardsIntermission());
    }

    private IEnumerator PlayCardsIntermission()
    {
        // Waiting for Entities abilities (ETB) being tracked 
        yield return new WaitForSeconds(SorsTimings.wait);

        // Waiting for CEH to set QueueResolving to false
        StartCoroutine(_cardEffectsHandler.StartResolvingQueue());
        while (_cardEffectsHandler.QueueResolving) 
            yield return new WaitForSeconds(0.1f);

        CheckPlayAnotherCard();
    }

    private void CheckPlayAnotherCard()
    {
        // Reset abilities and dead entities
        _boardManager.BoardCleanUp(_market.GetTileInfos(), false);

        // Add each player that skipped to _readyPlayers
        foreach (var player in _gameManager.players.Keys) 
        {
            if (_skippedPlayers.Contains(player)) _readyPlayers.Add(player);
            else if (player.Plays == 0) _readyPlayers.Add(player);
        }

        // Play another card if not all players have skipped
        if (_readyPlayers.Count != _gameManager.players.Count) {
            _cardCollectionPanel.RpcSoftResetPanel();
            PlayCard();
        } else {
            FinishPlayCard();
        }
    }

    private void FinishPlayCard()
    {
        _cardCollectionPanel.RpcResetPanel();
        _boardManager.ShowHolders(false);
        PlayersStatsResetAndDiscardMoney();

        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    private void Combat() => combatManager.UpdateCombatState(CombatState.Attackers);
    public void FinishCombat() => UpdateTurnState(TurnState.NextPhase);

    #region Prevail
    private void Prevail()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            // Starts phase on clients individually with 2nd argument = true for the bonus
            _prevailPanel.TargetBeginPrevailPhase(player.connectionToClient, player.chosenPhases.Contains(Phase.Prevail));
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
        print($"NextPrevailOption - {_prevailOptionsToPlay.Count} remaining");
        if (_prevailOptionsToPlay.Count != 0) print(_prevailOptionsToPlay[0]);

        // All options have been played
        if (_prevailOptionsToPlay.Count == 0)
        {
            PrevailCleanUp();
            return;
        }

        var nextOption = _prevailOptionsToPlay[0];
        _prevailOptionsToPlay.RemoveAt(0);

        switch (nextOption)
        {
            case PrevailOption.CardSelection:
                turnState = TurnState.CardIntoHand;
                StartPrevailInteraction(PrevailOption.CardSelection);
                break;
            case PrevailOption.Trash:
                turnState = TurnState.Trash;
                StartPrevailInteraction(PrevailOption.Trash);
                break;
            case PrevailOption.Score:
                print("<color=yellow> Prevail score not implemented yet </color>");
                NextPrevailOption();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StartPrevailInteraction(PrevailOption currentPrevailOption)
    {
        foreach (var cardsList in _selectedCards.Values) cardsList.Clear();

        ShowCardCollection();
        foreach (var (player, options) in _playerPrevailOptions)
        {
            var nbPicks = options.Count(option => option == currentPrevailOption);
            _cardCollectionPanel.TargetBeginPrevailSelection(player.connectionToClient, turnState, nbPicks);
        }
    }

    public void PlayerSelectedPrevailCards(PlayerManager player, List<GameObject> selectedCards)
    {
        _selectedCards[player] = selectedCards;
        PlayerIsReady(player);
    }

    public void PlayerSkipsPrevailOption(PlayerManager player) => PlayerIsReady(player);

    private void FinishPrevailCardIntoHand()
    {
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards)
            {
                var stats = card.GetComponent<CardStats>();
                player.discard.Remove(stats.cardInfo);
                player.hand.Add(stats.cardInfo);
                player.RpcMoveCard(card, CardLocation.Discard, CardLocation.Hand);
            }
        }

        _cardCollectionPanel.RpcResetPanel();
        NextPrevailOption();
    }

    private void FinishPrevailTrash()
    {
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards)
            {
                card.GetComponent<NetworkIdentity>().RemoveClientAuthority();

                var cardInfo = card.GetComponent<CardStats>().cardInfo;
                player.hand.Remove(cardInfo);
                player.RpcMoveCard(card, CardLocation.Hand, CardLocation.Trash);
            }
        }

        _cardCollectionPanel.RpcResetPanel();
        NextPrevailOption();
    }

    private void PrevailCleanUp()
    {
        _prevailPanel.RpcReset();
        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion
    
    #region EndPhase
    private void CleanUp()
    {
        if (GameEnds())
        {
            _gameManager.EndGame();
            UpdateTurnState(TurnState.Idle);
            return;
        }

        _readyPlayers.Clear();
        StartCoroutine(CleanUpIntermission());
    }

    private IEnumerator CleanUpIntermission()
    {
        // TODO: Should not use _market here but access it from _boardManager directly
        _boardManager.BoardCleanUp(_market.GetTileInfos(), true);
        OnPhaseChanged?.Invoke(TurnState.CleanUp);
        PlayersStatsResetAndDiscardMoney();

        yield return new WaitForSeconds(SorsTimings.turnStateTransition);

        UpdateTurnState(TurnState.PhaseSelection);
    }

    private bool GameEnds()
    {
        var gameEnds = false;
        foreach (var (player, health) in _gameManager.players.Keys.Select(player => (player, player.Health)))
        {
            if (health > 0) continue;

            OnPlayerDies?.Invoke(player);
            gameEnds = true;
        }

        return gameEnds;
    }

    #endregion

    #region HelperFunctions

    private void ShowCardCollection()
    {
        foreach (var player in _gameManager.players.Keys)
        {
            List<CardInfo> cards = player.hand; // mostly will be the hand cards

            // other / more specific options
            if (turnState == TurnState.CardIntoHand) cards = player.discard;
            else if (turnState == TurnState.Develop) cards = cards.Where(card => card.type == CardType.Technology).ToList();
            else if (turnState == TurnState.Deploy) cards = cards.Where(card => card.type == CardType.Creature).ToList();

            var cardObjects = GameManager.CardInfosToGameObjects(cards);
            // Need plays here because clients can't access that number
            _cardCollectionPanel.TargetShowCardCollection(player.connectionToClient, turnState, cardObjects, cards, player.Plays);

            // Reevaluating money in play to highlight playable cards after reset
            if (turnState == TurnState.Develop || turnState == TurnState.Deploy)
                _cardCollectionPanel.TargetCheckPlayability(player.connectionToClient, player.Cash);
        }
    }

    public void PlayerIsReady(PlayerManager player)
    {
        if (!_readyPlayers.Contains(player)) _readyPlayers.Add(player);
        if (_readyPlayers.Count < _nbPlayers) return;
        _readyPlayers.Clear();

        switch (turnState)
        {
            case TurnState.PhaseSelection:
                FinishPhaseSelection();
                break;
            case TurnState.Discard:
                FinishDiscard();
                break;
            case TurnState.Invent or TurnState.Recruit:
                BuyCards();
                break;
            case TurnState.Develop or TurnState.Deploy:
                PlayEntities();
                break;
            case TurnState.Prevail:
                StartPrevailOptions();
                break;
            case TurnState.CardIntoHand:
                FinishPrevailCardIntoHand();
                break;
            case TurnState.Trash:
                FinishPrevailTrash();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        switch (turnState)
        {
            case TurnState.Invent or TurnState.Recruit:
                _market.TargetCheckMarketPrices(player.connectionToClient, newAmount);
                break;
            case TurnState.Develop or TurnState.Deploy:
                _cardCollectionPanel.TargetCheckPlayability(player.connectionToClient, newAmount);
                break;
        }
    }

    private void PlayersStatsResetAndDiscardMoney()
    {
        // _logger.RpcLog("... Discarding money ...", LogType.Standard);
        _handManager.RpcHighlightMoney(false);

        foreach (var player in _gameManager.players.Keys)
        {
            // Returns unused money then discards the remaining cards
            player.ReturnMoneyToHand(false);
            player.Cash = 0;
        }
    }

    public void ForceEndTurn()
    { // experimental
        _handManager.RpcHighlightMoney(false);
        _cardCollectionPanel.RpcResetPanel();
        _market.RpcEndMarketPhase();
        _boardManager.ShowHolders(false);

        combatManager.CombatCleanUp(true);

        _prevailPanel.RpcOptionsSelected();
        _prevailPanel.RpcReset();

        CleanUp();
    }
    #endregion

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
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
    Combat,
    Recruit,
    Deploy,
    Prevail,
    Trash,
    CardIntoHand,
    CleanUp
}

public enum Phase : byte
{
    Draw,
    Invent,
    Develop,
    Combat,
    Recruit,
    Deploy,
    Prevail,
    None,
}
