using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

[RequireComponent(typeof(CardSelectionHandler))]
public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    public PlayerManager LocalPlayer { get; private set; }
    private CardSelectionHandler _selectionHandler;
    private BoardManager _boardManager;
    [SerializeField] private ArrowManager _arrowManager;
    [SerializeField] private CardPile[] _interactablePiles;
    private InteractionUI _interactionUI;

    [Header("Helper Fields")]
    private InteractionStateBase _currentState;
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

    public void ConfirmCurrentState()
    {
        _selectionHandler.EndSelection();
        _currentState.HandleConfirm(this);
    }

    public void SkipCurrentState() => _currentState.HandleSkip(this);
    public void ResetCurrentState() => _currentState.HandleReset(this);

    private void SetCurrentTurnState(TurnState turnState)
    {
        _currentState = (InteractionStateBase) _interactionStates.FirstOrDefault(x => x.Config.turnState == turnState);
        if(_currentState == null)
        {
            Debug.LogError($"No interaction state found for {turnState}");
            return;
        }
    }

    [ClientRpc]
    public void RpcFinishState()
    {
        print("    - InteractionPanel: Reset panel");
        _currentState.EndState();
        _selectionHandler.EndSelection();
    }
    #region Combat

    [Command(requiresAuthority = false)]
    private void CmdSetGroupTarget(BattleZoneEntity target, List<CreatureEntity> creatures)
    {
        if (_currentState.Config.turnState == TurnState.Attackers) _boardManager.PlayerChoosesTargetToAttack(target, creatures);
        else if (_currentState.Config.turnState == TurnState.Blockers) _boardManager.PlayerChoosesAttackerToBlock(target.GetComponent<CreatureEntity>(), creatures);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerConfirmsCombat(PlayerManager player) => _boardManager.PlayerConfirmsCombatState(player);

    [Command(requiresAuthority = false)]
    private void CmdResetArrows(PlayerManager player) 
    {
        _arrowManager.TargetResetArrows(player.connectionToClient);
    }

    #endregion

    #region === State Context API ===
    // Everything below is intentionally narrow & safe

    public Stack<CardStats> SelectedCards => _selectionHandler.selectedCards;
    public InteractionStateConfig CurrentConfig => _currentState.Config;
    public ArrowManager Arrows => _arrowManager;

    public void ConfirmCardSelection()
    {
        LocalPlayer.CmdConfirmSelection(
            SelectedCards.Select(c => c.cardInfo.goID).ToList()
        );
    }

    public void SkipInteraction()
    {
        LocalPlayer.CmdSkipInteraction();
        _selectionHandler.SkipCardInteraction();
    }

    public void ConfirmCashSpending(bool isBuy) {
        LocalPlayer.Cards.ConfirmMoneyCards();
        LocalPlayer.ConfirmPayment(_selectionHandler.cardSelection, isBuy);
    }

    public void UndoMoneyPlay() => _selectionHandler.UndoMoneyPlay(CurrentConfig.interactionType);
    public void ResetCombatArrows() => CmdResetArrows(LocalPlayer);
    public void ConfirmCombatSelection()
    {
        foreach(var (target, creatureList) in _arrowManager.GetPlayerSelection())
            CmdSetGroupTarget(target, creatureList);
        
        CmdPlayerConfirmsCombat(LocalPlayer);
    }

    #endregion
}