using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasePanel : NetworkBehaviour
{
    public static PhasePanel Instance { get; private set; }
    [SerializeField] private List<Phase> _selectedPhases = new();
    private int _nbPhasesToChose;
    [SerializeField] private PhaseItemUI attack;
    [SerializeField] private PhaseItemUI block;
    public static event Action OnPhaseSelectionStarted;
    public static event Action OnPhaseSelectionConfirmed;

    [Header("UI Elements")]
    [SerializeField] private Button confirm;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text actionDescriptionText;

    [Header("Overlay turn screen")]
    [SerializeField] private TMP_Text overlayTurnText;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Color overlayImageColor;
    [SerializeField] private int overlayScreenWaitTime = 1;
    [SerializeField] private float overlayScreenFadeTime = 0.5f;
    private bool _animate;
    
    private void Awake() {
        if (!Instance) Instance = this;
    }

    #region Prepare and Phase Selection
    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhasesToChose, bool animations){
        _nbPhasesToChose = nbPhasesToChose;
        _animate = animations;

        DropZoneManager.OnStartAttacking += BeginCombatAttack;
        DropZoneManager.OnStartBlocking += BeginCombatBlock;
        // BoardManager.OnBlockersDeclared += TargetEndCombatBlockers;
    }


    [ClientRpc]
    public void RpcBeginPhaseSelection(int currentTurn){
        turnText.text = "Turn " + currentTurn.ToString();
        OnPhaseSelectionStarted?.Invoke();

        if(!_animate) return;
        overlayTurnText.text = "Turn " + currentTurn.ToString();
        StartCoroutine(WaitAndFade());
    }

    public void UpdateSelectedPhase(Phase phase){
        if (_selectedPhases.Contains(phase)){
            _selectedPhases.Remove(phase);
        } else {
            _selectedPhases.Add(phase);
        }
        
        confirm.interactable = _selectedPhases.Count == _nbPhasesToChose;
    }
    #endregion

    #region Combat

    [ClientRpc]
    public void RpcStartCombatPhase(CombatState state){
        if (state == CombatState.Attackers) BeginCombatAttack();
        else if (state == CombatState.Blockers) BeginCombatBlock();
    }
    
    private void BeginCombatAttack(){
        actionDescriptionText.text = "Select attackers";
        attack.StartSelection();
    }
    private void BeginCombatBlock(){
        actionDescriptionText.text = "Select blockers";
        block.StartSelection();
    }

    [TargetRpc]
    public void TargetDisableCombatButtons(NetworkConnection conn){
        attack.Reset();
        block.Reset();
    }

    public void PlayerPressedCombatButton(){
        var player = PlayerManager.GetLocalPlayer();
        player.PlayerPressedCombatButton();
    }

    #endregion

    public void ConfirmButtonPressed(){
        actionDescriptionText.text = "Wait for opponent...";
        confirm.interactable = false;

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }

    [ClientRpc]
    public void RpcChangeActionDescriptionText(TurnState state){
        var text = state switch {
            TurnState.PhaseSelection => "Select " + _nbPhasesToChose.ToString() + " phases",
            TurnState.Discard => "Discard cards",
            TurnState.Invent => "Buy developments",
            TurnState.Develop => "Play developments",
            TurnState.Recruit => "Buy creatures",
            TurnState.Deploy => "Play creatures",
            TurnState.Prevail => "Choose prevail options",
            TurnState.Trash => "Trash cards",
            _ => ""
        };

        actionDescriptionText.text = text;
    }

    private IEnumerator WaitAndFade() {

        overlayImage.enabled = true;
        
        // Wait and fade
        yield return new WaitForSeconds(overlayScreenWaitTime);
        overlayImage.CrossFadeAlpha(0f, overlayScreenFadeTime, false);
        overlayTurnText.text = "";

        // Wait and disable
        yield return new WaitForSeconds(overlayScreenFadeTime);

        overlayImage.enabled = false;
        overlayImage.CrossFadeAlpha(1f, 0f, false);
    }

    private void OnDestroy() {
        DropZoneManager.OnStartAttacking -= BeginCombatAttack;
        DropZoneManager.OnStartBlocking -= BeginCombatBlock;
    }
}