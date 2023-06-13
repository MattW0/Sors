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
    [SerializeField] private Button confirm;
    [SerializeField] private PhaseItemUI attack;
    [SerializeField] private PhaseItemUI block;
    public static event Action OnPhaseSelectionStarted;
    public static event Action OnPhaseSelectionConfirmed;

    [Header("Turn screen")]
    private bool _animate;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text turnScreenText;
    [SerializeField] private GameObject turnScreen;
    private Image _backgroundImage;
    [SerializeField] private int turnScreenWaitTime = 1;
    [SerializeField] private float turnScreenFadeTime = 0.5f;
    
    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPreparePhasePanel(int nbPhasesToChose, bool animations){
        _nbPhasesToChose = nbPhasesToChose;
        _animate = animations;

        _backgroundImage = turnScreen.transform.GetChild(0).GetComponent<Image>();

        DropZoneManager.OnDeclareAttackers += BeginCombatAttack;
        DropZoneManager.OnDeclareBlockers += BeginCombatBlock;
        // BoardManager.OnBlockersDeclared += TargetEndCombatBlockers;
    }


    [ClientRpc]
    public void RpcBeginPhaseSelection(int currentTurn){
        turnText.text = "Turn " + currentTurn.ToString();
        OnPhaseSelectionStarted?.Invoke();

        if(!_animate) return;
        turnScreenText.text = "Turn " + currentTurn.ToString();
        StartCoroutine(WaitAndFade());
    }

    public void UpdateActive(Phase phase){
        if (_selectedPhases.Contains(phase)){
            _selectedPhases.Remove(phase);
        } else {
            _selectedPhases.Add(phase);
        }
        
        confirm.interactable = _selectedPhases.Count == _nbPhasesToChose;
    }

    public void BeginCombatAttack() => attack.StartSelection();

    public void BeginCombatBlock(){
        // attack.Reset();
        block.StartSelection();
    }

    [TargetRpc]
    public void TargetDisableButtons(NetworkConnection conn){
        attack.Reset();
        block.Reset();
    }

    public void PlayerPressedCombatButton(){
        var player = PlayerManager.GetLocalPlayer();
        player.PlayerPressedCombatButton();
    }

    public void ConfirmButtonPressed(){
        confirm.interactable = false;

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPhaseSelection(_selectedPhases);

        _selectedPhases.Clear();
        OnPhaseSelectionConfirmed?.Invoke();
    }

    private IEnumerator WaitAndFade() {

        turnScreen.SetActive(true);
        
        // Wait and fade
        yield return new WaitForSeconds(turnScreenWaitTime);
        _backgroundImage.CrossFadeAlpha(0f, turnScreenFadeTime, false);

        // Wait and disable
        yield return new WaitForSeconds(turnScreenFadeTime);
        turnScreen.SetActive(false);
    }

    private void OnDestroy() {
        DropZoneManager.OnDeclareAttackers -= BeginCombatAttack;
        DropZoneManager.OnDeclareBlockers -= BeginCombatBlock;
    }
}