using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

[RequireComponent(typeof(CardSelectionHandler))]
public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    private CardSelectionHandler _selectionHandler;
    private BoardManager _boardManager;
    [SerializeField] private ArrowManager _arrowManager;
    [SerializeField] private InteractionPileUI[] _interactablePiles;

    [Header("Helper Fields")]
    private InteractionStateBase _currentState;
    [SerializeField] private InteractionStateBase[] _interactionStates = {
        new DevelopInteractionState(),
        new DeployInteractionState(),
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _selectionHandler = GetComponent<CardSelectionHandler>();

        InteractionStateBase.OnSkipInteraction += OnSkip;
        InteractionStateBase.OnResetInteraction += OnReset;
        InteractionStateBase.OnConfirmInteraction += OnConfirm;
    }

    private void Start() 
    {
        // Load all interaction states from Resources folder
        // _interactionStates = Resources.LoadAll<InteractionStateBase>("InteractionStates");
        if (_interactionStates == null || _interactionStates.Length == 0)
        {
            Debug.LogError("No interaction states found in Resources/InteractionStates folder. Make sure to create the states and place them in the Resources folder.");
            return;
        }

        foreach(var state in _interactionStates) state.Initialize(_interactablePiles);
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel()
    {
        _boardManager = BoardManager.Instance;
        _selectionHandler.LocalPlayer = PlayerManager.GetLocalPlayer();
    }

    private void OnSkip()
    {
        // if (isCardInteraction) _selectionHandler.SkipCardInteraction();
        CmdPlayerSkips(_selectionHandler.LocalPlayer);
    }

    private void OnReset()
    {
        CmdPlayerResets(_selectionHandler.LocalPlayer);
    }

    private void OnConfirm()
    {
        // if (isCardInteraction) {
        //     _selectionHandler.ConfirmCardSelection();
        //     return;
        // }
        
        foreach(var (target, creatureList) in _arrowManager.GetPlayerSelection())
            CmdSetGroupTarget(target, creatureList);
        
        CmdPlayerConfirms(_selectionHandler.LocalPlayer);
    }

    [TargetRpc]
    public void TargetStartCardInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"    - InteractionPanel: Choose {numberSelections} / {interactableCards.Count} cards");

        SetCurrentTurnState(turnState);
        _currentState.StartInteraction(interactableCards, numberSelections);
        // OnInteractionBegin?.Invoke(_currentState, numberSelections, autoSkip);
    }

    [TargetRpc]
    internal void TargetStartCombatState(NetworkConnection conn, TurnState turnState, bool skip)
    {
        print($"    - InteractionPanel: Start combat state {turnState}");
        
        SetCurrentTurnState(turnState);
        _currentState.StartCombatInteraction(skip);

        // OnInteractionBegin?.Invoke(_currentState, -1, skip);
    }

    private void SetCurrentTurnState(TurnState turnState)
    {
        _currentState = (InteractionStateBase) _interactionStates.FirstOrDefault(x => x.config.turnState == turnState);
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

        // _playerHand.EndInteraction();
        // _playerDiscard.EndInteraction();
        
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
    private void CmdPlayerConfirms(PlayerManager player) => _boardManager.PlayerConfirmsCombatState(player);

    [Command(requiresAuthority = false)]
    private void CmdPlayerResets(PlayerManager player) => _arrowManager.TargetResetArrows(player.connectionToClient);

    [Command(requiresAuthority = false)]
    private void CmdPlayerSkips(PlayerManager player)
    {
        _arrowManager.TargetResetArrows(player.connectionToClient);
        _boardManager.PlayerConfirmsCombatState(player);
    }

    #endregion

    private void OnDestroy()
    {
        InteractionStateBase.OnSkipInteraction -= OnSkip;
        InteractionStateBase.OnResetInteraction -= OnReset;
        InteractionStateBase.OnConfirmInteraction -= OnConfirm;
    }
}