using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using NUnit.Framework.Constraints;

[RequireComponent(typeof(CardSelectionHandler))]
public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    public PlayerManager LocalPlayer { get; set; }
    private CardSelectionHandler _selectionHandler;
    private BoardManager _boardManager;
    [SerializeField] private ArrowManager _arrowManager;
    [SerializeField] private CardPile[] _interactablePiles;
    private InteractionUI _interactionUI;
    public static event Action<TurnState> OnUndoMoneyPlay;
    public static event Action OnConfirmPhaseSelection;
    public static event Action OnResetPhaseSelection;

    [Header("Helper Fields")]
    private IInteractionState _currentState;
    private readonly IInteractionState[] _interactionStates = {
        new PhaseSelectionState(),
        new DiscardState(),
        new InventState(),
        new DevelopState(),

        new AttackState(),
        new BlockState(),
        new DamageState(),

        new RecruitState(),
        new DeployState(),
        new PrevailToHandState(),
        new PrevailTrashState()
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _selectionHandler = GetComponent<CardSelectionHandler>();
        _interactionUI = GetComponentInChildren<InteractionUI>();

        InteractionStateBase.OnConfirmInteraction += PlayerConfirms;
        InteractionStateBase.OnSkipInteraction += PlayerSkips;
        InteractionStateBase.OnResetInteraction += PlayerResets;
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

    [ClientRpc]
    internal void RpcStartPhaseSelection()
    {
        SetCurrentTurnState(TurnState.PhaseSelection);
        _interactionUI.StartInteraction(_currentState, skip: false);
        _currentState.StartState();
    }

    [TargetRpc]
    public void TargetStartCardInteraction(NetworkConnection target, List<CardStats> interactableCards, TurnState turnState, int numberSelections)
    {
        print($"    - InteractionPanel: Choose {numberSelections} / {interactableCards.Count} cards");
        SetCurrentTurnState(turnState);

        var state = (CardInteractionState) _currentState;
        state.InitializeInteraction(interactableCards, numberSelections);
        var skip = state.CheckAutoskip();
        
        // Always start interaction UI so players know what's happening
        _interactionUI.StartInteraction(_currentState, skip, numberSelections);
        if(skip) return;

        _currentState.StartState();
        _selectionHandler.BeginInteraction(state, numberSelections);
        LocalPlayer.LocalCash = LocalPlayer.LocalCash;
    }

    [TargetRpc]
    internal void TargetStartCombatState(NetworkConnection target, TurnState turnState, bool skip)
    {
        print($"    - InteractionPanel: Start combat state {turnState}");
        SetCurrentTurnState(turnState);
        
        // Always start interaction UI so players know what's happening
        _interactionUI.StartInteraction(_currentState, skip);
        if(skip) return;
        
        _currentState.StartState();
    }

    private void SetCurrentTurnState(TurnState turnState)
    {
        _currentState = (InteractionStateBase) _interactionStates.FirstOrDefault(x => x.Config.turnState == turnState);
        if(_currentState == null)
        {
            Debug.LogError($"No interaction state found for {turnState}");
            return;
        }
    }

    private void PlayerConfirms(InteractionType type)
    {
        // print("Player confirms interaction type "+ type);
        _selectionHandler.EndSelection();
        
        // Default behavior that is resolved individually in TurnManager
        if (type == InteractionType.Select) LocalPlayer.CmdConfirmSelection(_selectionHandler.selectedCards.Select(card => card.cardInfo.goID).ToList());
        else if (type == InteractionType.Combat) ConfirmCombatSelection();
        
        // Interaction with playing money cards
        else if (type == InteractionType.Buy || type == InteractionType.Play) {
            LocalPlayer.Cards.ConfirmMoneyCards();
            LocalPlayer.ConfirmPayment(_selectionHandler.cardSelection, type);
        } else {
            OnConfirmPhaseSelection.Invoke();
        }
    }

    private void PlayerSkips(InteractionType type)
    {
        // We auto skip in 
        if(type == InteractionType.Combat) return;

        LocalPlayer.CmdSkipInteraction();
        _selectionHandler.SkipCardInteraction();
    }

    [ClientRpc]
    public void RpcFinishState()
    {
        print("    - InteractionPanel: Reset panel");
        _currentState.EndState();
        _selectionHandler.EndSelection();
    }
    #region Combat

    private void ConfirmCombatSelection()
    {
        foreach(var (target, creatureList) in _arrowManager.GetPlayerSelection())
            CmdSetGroupTarget(target, creatureList);
        
        CmdPlayerConfirmsCombat(LocalPlayer);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetGroupTarget(BattleZoneEntity target, List<CreatureEntity> creatures)
    {
        if (_currentState.Config.turnState == TurnState.Attackers) _boardManager.PlayerChoosesTargetToAttack(target, creatures);
        else if (_currentState.Config.turnState == TurnState.Blockers) _boardManager.PlayerChoosesAttackerToBlock(target.GetComponent<CreatureEntity>(), creatures);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerConfirmsCombat(PlayerManager player) => _boardManager.PlayerConfirmsCombatState(player);

    private void PlayerResets() 
    {
        if (_currentState.Config.turnState == TurnState.Attackers 
            || _currentState.Config.turnState == TurnState.Blockers)
            CmdResetArrows();
        else if (_currentState.Config.turnState == TurnState.PhaseSelection)
            OnResetPhaseSelection?.Invoke();
        else 
            UndoMoneyPlay();
    }
    [Command(requiresAuthority = false)]
    private void CmdResetArrows() 
    {
        _arrowManager.TargetResetArrows(LocalPlayer.connectionToClient);
    }

    private void UndoMoneyPlay()
    {
        _selectionHandler.UndoMoneyPlay(_currentState.Config.interactionType);
        OnUndoMoneyPlay?.Invoke(_currentState.Config.turnState);
    }

    #endregion

    private void OnDestroy()
    {
        InteractionStateBase.OnConfirmInteraction -= PlayerConfirms;
        InteractionStateBase.OnSkipInteraction -= PlayerSkips;
        InteractionStateBase.OnResetInteraction -= PlayerResets;
    }
}