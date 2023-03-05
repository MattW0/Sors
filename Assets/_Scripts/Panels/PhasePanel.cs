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
    public List<Phase> _selectedPhases = new();
    private int _nbPhasesToChose;
    public bool disableSelection { get; private set; }
    [SerializeField] private GameObject maxView;
    [SerializeField] private Button confirm;
    public static event Action OnPhaseSelectionEnded;

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
    public void RpcPreparePhasePanel(int nbPhasesToChose, bool animations)
    {
        _nbPhasesToChose = nbPhasesToChose;
        _animate = animations;

        _backgroundImage = turnScreen.transform.GetChild(0).GetComponent<Image>();
    }


    [ClientRpc]
    public void RpcBeginPhaseSelection(int currentTurn){
        maxView.SetActive(true);

        if(!_animate) return;
        turnText.text = "Turn " + currentTurn.ToString();
        StartCoroutine(WaitAndFade());
    }

    private IEnumerator WaitAndFade() {

        turnScreen.SetActive(true);
        
        // Wait and fade
        yield return new WaitForSeconds(turnScreenWaitTime);
        _backgroundImage.CrossFadeAlpha(0f, turnScreenFadeTime, false);
        turnText.CrossFadeAlpha(0f, turnScreenFadeTime, false);

        // Wait and disable
        yield return new WaitForSeconds(turnScreenFadeTime);
        turnScreen.SetActive(false);
    }

    public void UpdateActive(Phase phase)
    {
        if (_selectedPhases.Contains(phase))
        {
            _selectedPhases.Remove(phase);
            return;
        }
        
        _selectedPhases.Add(phase);
        confirm.interactable = _selectedPhases.Count == _nbPhasesToChose;
    }

    public void ConfirmButtonPressed(){
        disableSelection = true;
        confirm.interactable = false;

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPhaseSelection(_selectedPhases);
    }

    [ClientRpc]
    public void RpcEndPhaseSelection(){
        maxView.SetActive(false);
        disableSelection = false;

        OnPhaseSelectionEnded?.Invoke();
        _selectedPhases.Clear();
    }
}