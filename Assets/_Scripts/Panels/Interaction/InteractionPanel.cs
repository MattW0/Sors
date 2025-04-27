using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

[RequireComponent(typeof(CardSelectionHandler))]
public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    public PlayerManager LocalPlayer { get; set; }
    private CardSelectionHandler _selectionHandler;
    private BoardManager _boardManager;
    [SerializeField] private ArrowManager _arrowManager;
    [SerializeField] private InteractionPileUI[] _interactablePiles;
    private InteractionUI _interactionUI;

    [Header("Helper Fields")]
    private InteractionStateBase _currentState;
    private readonly InteractionStateBase[] _interactionStates = {
        new DiscardState(),
        new DevelopState(),
        new DeployState(),
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _selectionHandler = GetComponent<CardSelectionHandler>();
        _interactionUI = GetComponentInChildren<InteractionUI>();

        InteractionStateBase.OnSkipInteraction += PlayerSkips;
        InteractionStateBase.OnResetInteraction += PlayerResets;
        InteractionStateBase.OnConfirmInteraction += PlayerConfirms;
    }

    private void Start() 
    {
        foreach(var state in _interactionStates) state.Initialize(_interactablePiles);
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel()
    {
        _boardManager = BoardManager.Instance;
        LocalPlayer = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetStartCardInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"    - InteractionPanel: Choose {numberSelections} / {interactableCards.Count} cards");

        SetCurrentTurnState(turnState);
        var autoSkip = _currentState.StartInteraction(interactableCards, numberSelections);
        _interactionUI.StartInteraction(_currentState, numberSelections, autoSkip);
        _selectionHandler.BeginInteraction(_currentState, numberSelections);
    }

    [TargetRpc]
    internal void TargetStartCombatState(NetworkConnection target, TurnState turnState, bool skip)
    {
        print($"    - InteractionPanel: Start combat state {turnState}");
        
        SetCurrentTurnState(turnState);
        _currentState.StartCombatInteraction(skip);

        // OnInteractionBegin?.Invoke(_currentState, -1, skip);
    }

    private void PlayerSkips()
    {
        _selectionHandler.SkipCardInteraction();
        CmdPlayerSkips();
    }

    private void PlayerResets() => CmdPlayerResets();

    private void PlayerConfirms(InteractionType type)
    {
        print("Player confirms interaction type "+ type);
        if (type == InteractionType.Select) ConfirmCardSelection();
        else if (type == InteractionType.Buy) ConfirmBuy();
        else if (type == InteractionType.Combat) ConfirmCombatSelection();
    }

    private void ConfirmCardSelection() => LocalPlayer.CmdConfirmSelection(_selectionHandler.selectedCards);
    private void ConfirmBuy() => LocalPlayer.CmdConfirmBuy(_selectionHandler.marketSelection);
    private void ConfirmCombatSelection()
    {
        foreach(var (target, creatureList) in _arrowManager.GetPlayerSelection())
            CmdSetGroupTarget(target, creatureList);
        
        CmdPlayerConfirms();
    }

    private void SetCurrentTurnState(TurnState turnState)
    {
        _currentState = _interactionStates.FirstOrDefault(x => x.config.turnState == turnState);
        if(_currentState == null)
        {
            Debug.LogError($"No interaction state found for {turnState}");
            return;
        }
    }

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, int cash)
    {
        if (_currentState == null) return;

        var allowedType = _currentState.config.turnState switch
        {
            TurnState.Develop => CardType.Technology,
            TurnState.Deploy => CardType.Creature,
            _ => CardType.None
        };

        _currentState.CheckPlayability(allowedType, cash);
    }

    [ClientRpc]
    public void RpcResetPanel()
    {
        print("    - InteractionPanel: Reset panel");
        _currentState.Reset();
        _selectionHandler.EndSelection();
    }

    // [TargetRpc]
    // public void TargetUndoMoneyPlay(NetworkConnection target) => MoneyCardsAreInteractable();
    public void SelectMarketTile(MarketTile tile) => _selectionHandler.SelectMarketTile(tile);
    public void DeselectMarketTile() => _selectionHandler.DeselectMarketTile();

    #region Combat

    [Command(requiresAuthority = false)]
    private void CmdSetGroupTarget(BattleZoneEntity target, List<CreatureEntity> creatures)
    {
        if (_currentState.config.turnState == TurnState.Attackers) _boardManager.PlayerChoosesTargetToAttack(target, creatures);
        else if (_currentState.config.turnState == TurnState.Blockers) _boardManager.PlayerChoosesAttackerToBlock(target.GetComponent<CreatureEntity>(), creatures);
    } 

    [Command(requiresAuthority = false)]
    private void CmdPlayerConfirms() => _boardManager.PlayerConfirmsCombatState(LocalPlayer);

    [Command(requiresAuthority = false)]
    private void CmdPlayerResets() => _arrowManager.TargetResetArrows(LocalPlayer.connectionToClient);

    [Command(requiresAuthority = false)]
    private void CmdPlayerSkips()
    {
        LocalPlayer.CmdSkipInteraction();
        _arrowManager.TargetResetArrows(LocalPlayer.connectionToClient);
        _boardManager.PlayerConfirmsCombatState(LocalPlayer);
    }

    #endregion

    private void OnDestroy()
    {
        InteractionStateBase.OnSkipInteraction -= PlayerSkips;
        InteractionStateBase.OnResetInteraction -= PlayerResets;
        InteractionStateBase.OnConfirmInteraction -= PlayerConfirms;
    }
}