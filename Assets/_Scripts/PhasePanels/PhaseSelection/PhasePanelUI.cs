using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanelUI : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Progress Bar")]
    [SerializeField] private Transform progressBar;
    [SerializeField] private Image progressBarHighlight;
    private float[] progressBarCheckpoints = {0.05f, 0.15f, 0.27f, 0.345f, 0.465f, 0.54f, 0.655f, 0.735f, 0.85f, 1f};

    [SerializeField] private List<Image> phaseHighlights;
    private Image _oldHighlight;
    private Image _newHighlight;
    public static event Action<Phase> OnPhaseSelectionConfirmed;

    private void Start()
    {
        // Reset cleanup highlight and start at phase selection (index 0)
        _oldHighlight = phaseHighlights[^1];
        UpdatePhaseHighlight(0);
        progressBarHighlight.color = SorsColors.phaseHighlight;
    }

    public void ShowOpponentChoices(Phase[] phases)
    {
        foreach(var phase in phases) 
            OnPhaseSelectionConfirmed?.Invoke(phase);
    }

    #region Phase Highlights
    public void UpdatePhaseHighlight(TurnState newState)
    {
        var newHighlightIndex = newState switch
        {
            TurnState.PhaseSelection => 0,
            TurnState.Draw => 1,
            TurnState.Invent => 2,
            TurnState.Develop => 3,
            TurnState.Attackers => 4,
            TurnState.Blockers => 5,
            TurnState.Recruit => 6,
            TurnState.Deploy => 7,
            TurnState.Prevail => 8,
            TurnState.CleanUp => 9,
            _ => -1
        };

        if (newHighlightIndex == -1) return;
        
        HighlightTransition(newHighlightIndex);
    }
    
    private void HighlightTransition(int newIndex, bool phaseSelection=false)
    {
        _oldHighlight.CrossFadeAlpha(0f, fadeDuration, false);
        if (!phaseSelection) phaseHighlights[newIndex].CrossFadeAlpha(1f, fadeDuration, false);

        _oldHighlight = phaseHighlights[newIndex];
        progressBar.localScale = new Vector3(progressBarCheckpoints[newIndex], 1f, 1f);
    }
    #endregion
}
