using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanelUI : MonoBehaviour
{
    [SerializeField] private NonOptionalPhaseItemUI attack;
    [SerializeField] private NonOptionalPhaseItemUI block;

    [Header("Progress Bar")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Transform progressBar;
    [SerializeField] private Image progressBarHighlight;
    private readonly float[] _progressBarCheckpoints = {0.04f, 0.15f, 0.265f, 0.345f, 0.46f, 0.54f, 0.655f, 0.735f, 0.85f, .96f};
    private List<IHighlightable> _phaseHighlights = new();
    private IHighlightable _oldHighlight;
    public static event Action<TurnState> OnPhaseSelectionConfirmed;

    private void Start()
    {
        // Reset cleanup highlight and start at phase selection (index 0)
        _phaseHighlights = GetComponentsInChildren<IHighlightable>().ToList();

        HighlightTransition(0);
    }

    public void ShowOpponentChoices(TurnState[] phases)
    {
        foreach(var phase in phases) 
            OnPhaseSelectionConfirmed?.Invoke(phase);
    }
    
    public void UpdatePhaseHighlight(TurnState newState)
    {
        var newHighlightIndex = GetIndex(newState);
        if (newHighlightIndex == -1) return;
        
        _oldHighlight.Disable(fadeDuration);
        HighlightTransition(newHighlightIndex);
    }
    
    private void HighlightTransition(int newIndex)
    {
        _phaseHighlights[newIndex].Highlight(1f, fadeDuration);
        _oldHighlight = _phaseHighlights[newIndex];
        progressBar.localScale = new Vector3(_progressBarCheckpoints[newIndex], 1f, 1f);
    }

    internal void HighlightPhasesToPlay(TurnState[] phases)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            Enum.TryParse(phases[i].ToString(), out TurnState nextTurnState);
            _phaseHighlights[GetIndex(nextTurnState)].Highlight(0.7f, fadeDuration);
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

    internal void StartCombatPhase(TurnState state)    
    {
        foreach(var p in _phaseHighlights) p.TooltipDisabled = true;

        if (state == TurnState.Attackers) attack.IsSelectable = true;
        else if (state == TurnState.Blockers) block.IsSelectable = true;
    }

    internal void DisableCombatButtons()
    {
        foreach(var p in _phaseHighlights) p.TooltipDisabled = false;

        attack.IsSelectable = false;
        block.IsSelectable = false;
    }
}
