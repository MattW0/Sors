using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanelUI : MonoBehaviour
{
    public static PhasePanelUI Instance { get; private set; }
    [SerializeField] private Camera _cam;

    [Header("Player Settings")]
    private int _nbPlayers;
    [SerializeField] private float playerChoiceInactiveAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Progress Bar")]
    [SerializeField] private Transform progressBar;
    [SerializeField] private Image progressBarHighlight;
    private float[] progressBarCheckpoints = {0.05f, 0.15f, 0.27f, 0.345f, 0.465f, 0.54f, 0.655f, 0.735f, 0.85f, 1f};

    [SerializeField] private List<Image> phaseHighlights;
    private Image _oldHighlight;
    private Image _newHighlight;
    public static event Action<Phase> OnPhaseSelectionConfirmed;

    private void Awake(){
        if (!Instance) Instance = this;
        
        progressBarHighlight.color = SorsColors.phaseHighlight;
        PhasePanel.OnCombatStart += StartCombat;
        PhasePanel.OnCombatEnd += EndCombat;
    }

    public void PrepareUI(int nbPlayers){
        _nbPlayers = nbPlayers;

        // Reset cleanup highlight and start at phase selection (index 0)
        _oldHighlight = phaseHighlights[^1];
        UpdatePhaseHighlight(0);
    }
    public void StartCombat(){
        _cam.gameObject.transform.position = new Vector3(-0.55f, 8, 1.25f);
    }

    public void EndCombat(){
        _cam.gameObject.transform.position = new Vector3(0, 10, 0);
    }

    public void ShowOpponentChoices(Phase[] phases){
        foreach(var phase in phases){
            OnPhaseSelectionConfirmed?.Invoke(phase);
        }

    }

    #region Phase Highlights
    public void UpdatePhaseHighlight(int newHighlightIndex){
        switch (newHighlightIndex) {
            case -1:  // No highlightable phase
                return;
            default:
                progressBar.localScale = new Vector3(progressBarCheckpoints[newHighlightIndex], 1f, 1f);
                break;
        }

        _newHighlight = phaseHighlights[newHighlightIndex];
        HighlightTransition(_oldHighlight, _newHighlight);
        _oldHighlight = _newHighlight;
    }
    
    private void HighlightTransition(Graphic oldImg, Graphic newImg, bool phaseSelection=false){
        oldImg.CrossFadeAlpha(0f, fadeDuration, false);

        if (phaseSelection) return;
        
        newImg.CrossFadeAlpha(1f, fadeDuration, false);
    }
    #endregion

    private void OnDestroy(){
        PhasePanel.OnCombatStart -= StartCombat;
        PhasePanel.OnCombatEnd -= EndCombat;
    }
}
