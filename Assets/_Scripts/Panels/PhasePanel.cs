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

        CombatManager.OnDeclareAttackers += RpcBeginCombatAttack;
        CombatManager.OnDeclareBlockers += RpcBeginCombatBlock;
        CombatManager.OnCombatResolved += RpcCombatEnded;
    }


    [ClientRpc]
    public void RpcBeginPhaseSelection(int currentTurn){
        turnText.text = "Turn " + currentTurn.ToString();
        OnPhaseSelectionStarted?.Invoke();

        if(!_animate) return;
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

    [ClientRpc]
    public void RpcBeginCombatAttack() => attack.StartSelection();

    [ClientRpc]
    public void RpcBeginCombatBlock(){
        attack.Reset();
        block.StartSelection();
    }

    [ClientRpc]
    public void RpcCombatEnded() => block.Reset();

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
}