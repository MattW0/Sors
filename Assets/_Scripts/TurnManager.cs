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
    private HandInteractionPanel _cardCollectionPanel;
    private PhasePanel _phasePanel;
    private PrevailPanel _prevailPanel;
    private PlayerInterfaceManager _playerInterfaceManager;
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
    private int _nbPlayers;
    private Dictionary<PlayerManager, int> _playerHealth = new();
    public int GetHealth(PlayerManager player) => _playerHealth[player];

    [Header("Helper Fields")]
    private Dictionary<PlayerManager, List<CardInfo>> _selectedKingdomCards = new();
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
                KingdomPhase();
                break;
            case TurnState.Develop:
                PlayCard();
                break;
            case TurnState.Combat:
                Combat();
                break;
            case TurnState.Recruit:
                KingdomPhase();
                break;
            case TurnState.Deploy:
                PlayCard();
                break;
            case TurnState.Prevail:
                Prevail();
                break;
            // --- Win check and turn reset ---
            case TurnState.CleanUp:
                CleanUp();
                break;
            case TurnState.Idle:
                _playerInterfaceManager.RpcLog("Game finished", LogType.Standard);
                break;

            default:
                print("<color=red>Invalid turn state</color>");
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        _phasePanel.RpcChangeActionDescriptionText(turnState);
    }

    private void Prepare(int nbPlayers)
    {
        _gameManager = GameManager.Instance;
        _handManager = Hand.Instance;
        _boardManager = BoardManager.Instance;
        _cardEffectsHandler = CardEffectsHandler.Instance;
        _kingdom = Kingdom.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;

        // Panels with setup (GameManager handles kingdom setup)
        _cardCollectionPanel = HandInteractionPanel.Instance;
        _cardCollectionPanel.RpcPrepareCardCollectionPanel(_gameManager.nbDiscard);
        _phasePanel = PhasePanel.Instance;
        _phasePanel.RpcPreparePhasePanel(nbPlayers, _gameManager.nbPhasesToChose, _gameManager.animations);
        _prevailPanel = PrevailPanel.Instance;
        _prevailPanel.RpcPreparePrevailPanel(_gameManager.prevailOptionsToChoose, _gameManager.prevailExtraOptions);

        _nbPlayers = nbPlayers;
        // print($"TurnManager prepare for {_nbPlayers} players");
        foreach (var player in _gameManager.players.Keys)
        {
            _playerHealth.Add(player, _gameManager.startHealth);
            _playerPhaseChoices.Add(player, new Phase[_gameManager.nbPhasesToChose]);
            _playerPrevailOptions.Add(player, new List<PrevailOption>());
            _selectedCards.Add(player, new List<GameObject>());
        }
        // reverse order of _playerPhaseChoices to have host first
        _playerPhaseChoices = _playerPhaseChoices.Reverse().ToDictionary(x => x.Key, x => x.Value);
        _playerPrevailOptions = _playerPrevailOptions.Reverse().ToDictionary(x => x.Key, x => x.Value);
        PlayerManager.OnCashChanged += PlayerCashChanged;

        UpdateTurnState(TurnState.PhaseSelection);
    }

    #region PhaseSelection
    private void PhaseSelection()
    {
        _gameManager.turnNb++;
        _playerInterfaceManager.RpcLog($" ------------ Turn {_gameManager.turnNb} ------------ ", LogType.TurnChange);
        OnPhaseChanged?.Invoke(TurnState.PhaseSelection);

        // Fix draw per turn
        foreach (var player in _gameManager.players.Keys)
            player.DrawCards(_gameManager.fixCardDraw);

        _phasePanel.RpcBeginPhaseSelection(_gameManager.turnNb);
    }

    public void PlayerSelectedPhases(PlayerManager player, Phase[] phases)
    {
        _playerPhaseChoices[player] = phases;

        foreach (var phase in phases)
        {
            if (!phasesToPlay.Contains(phase))
            {
                phasesToPlay.Add(phase);
            }
        }

        PlayerIsReady(player);
    }

    private void FinishPhaseSelection()
    {
        // Combat each round
        phasesToPlay.Add(Phase.Combat);
        phasesToPlay.Sort();
        _playerInterfaceManager.RpcLog($"Phases to play: {string.Join(", ", phasesToPlay)}", LogType.Phase);

        foreach (var (player, phases) in _playerPhaseChoices)
        {
            foreach (var phase in phases)
            {
                // Player turn stats
                if (phase == Phase.Invent) player.Invents++;
                else if (phase == Phase.Develop) player.Develops++;
                else if (phase == Phase.Deploy) player.Deploys++;
                else if (phase == Phase.Recruit) player.Recruits++;
            }

            // Show opponent choices in PhaseVisuals
            if (_gameManager.players.Count > 1) player.RpcShowOpponentChoices(phases);
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
        _playerInterfaceManager.RpcLog($"Turn changed to {nextTurnState}", LogType.Phase);

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
            var nbCardDraw = _gameManager.phaseCardDraw;
            if (player.chosenPhases.Contains(Phase.Draw)) nbCardDraw += _gameManager.extraDraw;

            player.DrawCards(nbCardDraw);
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

    #region KingdomPase (Invent, Recruit)
    private void KingdomPhase()
    {

        // convert current turnState (TurnState enum) to Phase enum
        var phase = Phase.Recruit;
        if (turnState == TurnState.Invent) phase = Phase.Invent;

        _selectedKingdomCards.Clear();
        _handManager.RpcHighlightMoney(true);
        _kingdom.RpcBeginPhase(phase);

        foreach (var player in _gameManager.players.Keys)
        {
            if (player.chosenPhases.Contains(phase))
            {
                _kingdom.TargetKingdomBonus(player.connectionToClient, _gameManager.kingdomPriceReduction);
                _kingdom.TargetCheckPriceKingdomTile(player.connectionToClient, player.Cash);
            }
        }
    }

    public void PlayerGetsKingdomBonus(PlayerManager player, CardType type, int amount)
    {
        // Public because EffectHandler can call this
        _kingdom.TargetKingdomPriceReduction(player.connectionToClient, type, amount);
    }

    public void PlayerSelectedKingdomTile(PlayerManager player, CardInfo card, int cardCost)
    {

        if (turnState == TurnState.Invent) player.Invents--;
        else if (turnState == TurnState.Recruit) player.Recruits--;

        player.Cash -= cardCost;

        if (_selectedKingdomCards.ContainsKey(player))
            _selectedKingdomCards[player].Add(card);
        else
            _selectedKingdomCards.Add(player, new List<CardInfo> { card });

        int actionsLeft = 0;
        if (turnState == TurnState.Invent) actionsLeft = player.Invents;
        else if (turnState == TurnState.Recruit) actionsLeft = player.Recruits;
        _kingdom.TargetResetKingdom(player.connectionToClient, actionsLeft);

        // Waiting for player to use remaining recruit actions
        if (actionsLeft > 0) return;
        PlayerIsReady(player);
    }

    private void KingdomSpawnAndReset()
    {
        foreach (var (owner, cards) in _selectedKingdomCards)
        {
            foreach (var cardInfo in cards)
            {
                if (cardInfo.type == CardType.Creature)
                {
                    _gameManager.PlayerGainCreature(owner, cardInfo);

                    var nextTile = _gameManager.GetNewCreatureFromDb();
                    _kingdom.RpcReplaceRecruitTile(cardInfo.title, nextTile);
                }
                else if (cardInfo.type == CardType.Technology) _gameManager.PlayerGainTechnology(owner, cardInfo);
                else if (cardInfo.type == CardType.Money) _gameManager.PlayerGainMoney(owner, cardInfo);
                _playerInterfaceManager.RpcLog($"{owner.PlayerName} buys '{cardInfo.title}", LogType.CreatureBuy);
            }
        }

        foreach (var player in _gameManager.players.Keys)
        {
            player.RpcResolveCardSpawn(_gameManager.cardSpawnAnimations);
        }

        PlayersStatsResetAndDiscardMoney(false);
        _kingdom.RpcEndKingdomPhase();

        UpdateTurnState(TurnState.NextPhase);
    }

    #endregion

    #region PlayCardPhase (Develop, Deploy)

    private void PlayCard()
    {
        _boardManager.ShowHolders(true);
        foreach (var cardsList in _selectedCards.Values) cardsList.Clear();

        _handManager.RpcHighlightMoney(true);
        ShowCardCollection();
    }

    public void PlayerPlaysCard(PlayerManager player, GameObject card)
    {
        if (turnState == TurnState.Develop) player.Develops--;
        else if (turnState == TurnState.Deploy) player.Deploys--;
        player.Cash -= card.GetComponent<CardStats>().cardInfo.cost;

        _selectedCards[player].Add(card);
        PlayerIsReady(player);
    }

    public void PlayerSkipsCardPlay(PlayerManager player)
    {
        if (turnState == TurnState.Develop) player.Develops--;
        else if (turnState == TurnState.Deploy) player.Deploys--;
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
            _boardManager.BoardCleanUp(false);
            CheckPlayAnotherCard();
            return;
        } 
        
        StartCoroutine(PlayCardsIntermission());
    }

    private IEnumerator PlayCardsIntermission()
    {
        // Waiting for Entities abilities (ETB) being tracked 
        yield return new WaitForSeconds(0.5f);

        // Waiting for CEH to set QueueResolving to false
        StartCoroutine(_cardEffectsHandler.StartResolvingQueue());
        while (_cardEffectsHandler.QueueResolving) yield return new WaitForSeconds(0.1f);

        _boardManager.BoardCleanUp(false);
        CheckPlayAnotherCard();
    }

    private void CheckPlayAnotherCard()
    {
        // Reset
        var playAnotherCard = false;
        _readyPlayers.Clear();

        // Check if plays are still possible and if not, add player to _readyPlayers
        foreach (var player in _gameManager.players.Keys)
        {
            if (turnState == TurnState.Develop)
            {
                if (player.Develops > 0) playAnotherCard = true;
                else _readyPlayers.Add(player);
            }
            else if (turnState == TurnState.Deploy)
            {
                if (player.Deploys > 0) playAnotherCard = true;
                else _readyPlayers.Add(player);
            }
        }

        if (playAnotherCard)
        {
            _cardCollectionPanel.RpcSoftResetPanel();
            PlayCard();
        }
        else
        {
            FinishPlayCard();
        }
    }

    private void FinishPlayCard()
    {
        _cardCollectionPanel.RpcResetPanel();
        _boardManager.ShowHolders(false);
        PlayersStatsResetAndDiscardMoney(false);

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
        // _handManager.RpcResetHighlight();

        NextPrevailOption();
    }

    private void FinishPrevailTrash()
    {
        // var tempList = new List<GameObject>();
        foreach (var (player, cards) in _selectedCards)
        {
            foreach (var card in cards)
            {
                card.GetComponent<NetworkIdentity>().RemoveClientAuthority();

                var cardInfo = card.GetComponent<CardStats>().cardInfo;
                player.hand.Remove(cardInfo);
                player.RpcMoveCard(card, CardLocation.Hand, CardLocation.Trash);
                // tempList.Add(card);
            }
        }

        // foreach (var obj in tempList)
        // {
        //     // NetworkServer.Destroy(obj);
        //     // obj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        // }

        _cardCollectionPanel.RpcResetPanel();
        // _handManager.RpcResetHighlight();

        NextPrevailOption();
    }

    private void PrevailCleanUp()
    {
        _prevailPanel.RpcReset();
        UpdateTurnState(TurnState.NextPhase);
    }
    #endregion

    #region PlayerInputs

    public void PlayerStartSelectTarget(BattleZoneEntity entity, Ability ability)
    {
        var owner = entity.Owner;
        _boardManager.FindTargets(entity, ability.target);
        entity.TargetSpawnTargetArrow(owner.connectionToClient);
        owner.TargetPlayerStartChooseTarget();
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
        OnPhaseChanged?.Invoke(TurnState.CleanUp);
        _boardManager.BoardCleanUp(true);
        PlayersStatsResetAndDiscardMoney(endOfTurn: true);

        yield return new WaitForSeconds(1f);

        UpdateTurnState(TurnState.PhaseSelection);
    }

    private bool GameEnds()
    {
        var gameEnds = false;
        foreach (var (player, health) in _playerHealth)
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
                                                // if (turnState == TurnState.Discard || turnState == TurnState.Trash) cards = player.hand;

            // other / more specific options
            if (turnState == TurnState.CardIntoHand) cards = player.discard;
            else if (turnState == TurnState.Develop) cards = cards.Where(card => card.type == CardType.Technology).ToList();
            else if (turnState == TurnState.Deploy) cards = cards.Where(card => card.type == CardType.Creature).ToList();

            var cardObjects = GameManager.CardInfosToGameObjects(cards);
            _cardCollectionPanel.TargetShowCardCollection(player.connectionToClient, turnState, cardObjects, cards);

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
                KingdomSpawnAndReset();
                if (turnState == TurnState.Invent) player.Invents = 0;
                else if (turnState == TurnState.Recruit) player.Recruits = 0;
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

    // TODO: CHeck logic with game state loading. Move to gameManager?
    public void PlayerHealthChanged(PlayerManager player, int amount) => _playerHealth[player] -= amount;

    private void PlayerCashChanged(PlayerManager player, int newAmount)
    {
        switch (turnState)
        {
            case TurnState.Invent or TurnState.Recruit:
                _kingdom.TargetCheckPriceKingdomTile(player.connectionToClient, newAmount);
                break;
            case TurnState.Develop or TurnState.Deploy:
                _cardCollectionPanel.TargetCheckPlayability(player.connectionToClient, newAmount);
                break;
        }
    }

    private void PlayersStatsResetAndDiscardMoney(bool endOfTurn = false)
    {
        // _playerInterfaceManager.RpcLog("... Discarding money ...", LogType.Standard);
        _handManager.RpcHighlightMoney(false);

        foreach (var player in _gameManager.players.Keys)
        {
            // Returns unused money then discards the remaining cards
            player.ReturnMoneyToHand(false);
            player.Cash = 0;

            if (!endOfTurn) continue;
            player.Develops = _gameManager.turnDevelops;
            player.Invents = _gameManager.turnInvents;
            player.Deploys = _gameManager.turnDeploys;
            player.Recruits = _gameManager.turnRecruits;

            player.chosenPhases.Clear();
            player.chosenPrevailOptions.Clear();
        }
    }

    public void ForceEndTurn()
    { // experimental
        _handManager.RpcHighlightMoney(false);
        _cardCollectionPanel.RpcResetPanel();
        _kingdom.RpcEndKingdomPhase();
        _boardManager.ShowHolders(false);

        combatManager.ResolveCombat(true);

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
