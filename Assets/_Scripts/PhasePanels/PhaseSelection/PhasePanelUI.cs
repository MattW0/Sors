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
    public static event Action<Phase> OnPhaseSelectionConfirmed;

    private void Start()
    {
        // Reset cleanup highlight and start at phase selection (index 0)
        _oldHighlight = phaseHighlights[^1];
        HighlightTransition(0);
    }

    public void ShowOpponentChoices(Phase[] phases)
    {
        foreach(var phase in phases) 
            OnPhaseSelectionConfirmed?.Invoke(phase);
    }
    
    public void UpdatePhaseHighlight(TurnState newState)
    {
        var newHighlightIndex = GetIndex(newState);

        if (newHighlightIndex == -1) return;
        
        HighlightTransition(newHighlightIndex);
    }
    
    private void HighlightTransition(int newIndex)
    {
        _oldHighlight.CrossFadeAlpha(0f, fadeDuration, false);
        phaseHighlights[newIndex].CrossFadeAlpha(1f, fadeDuration, false);

        _oldHighlight = phaseHighlights[newIndex];
        progressBar.localScale = new Vector3(progressBarCheckpoints[newIndex], 1f, 1f);
    }

    internal void HighlightPhasesToPlay(Phase[] phases)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            Enum.TryParse(phases[i].ToString(), out TurnState nextTurnState);
            phaseHighlights[GetIndex(nextTurnState)].CrossFadeAlpha(0.7f, 0f, false);
        }
    }
    
    private int GetIndex(TurnState state)
    {
        return state switch
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
    }
}
