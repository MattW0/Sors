using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanelUI : MonoBehaviour
{
    public static PhasePanelUI Instance { get; private set; }

    [Header("Player Settings")]
    private int _nbPlayers;
    [SerializeField] private float playerChoiceInactiveAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Progress Bar")]
    [SerializeField] private Transform progressBar;
    [SerializeField] private Image progressBarHighlight;
    private float[] progressBarCheckpoints = {0.05f, 0.15f, 0.27f, 0.345f, 0.465f, 0.54f, 0.655f, 0.735f, 0.85f, 1f};

    [SerializeField] private List<Image> phaseHighlights;
    [SerializeField] private List<Image> playerChoiceHighlights;
    private Image _oldHighlight;
    private Image _newHighlight;

    private void Awake(){
        if (!Instance) Instance = this;
        
        progressBarHighlight.color = SorsColors.phaseHighlight;
    }

    public void PrepareUI(int nbPlayers){
        _nbPlayers = nbPlayers;

        // Reset cleanup highlight and start at phase selection (index 0)
        _oldHighlight = phaseHighlights[^1];
        UpdatePhaseHighlight(0);
        
        int i = 0;
        foreach (var img in playerChoiceHighlights) {
            if(!img) continue;

            if (i%2 == 0) img.color = SorsColors.playerOne;
            else img.color = SorsColors.playerTwo;

            img.enabled = false;
            i++;
        }
    }

    public void ShowOpponentChoices(Phase[] phases){
        foreach(var phase in phases){
            var index = (int) phase;
            index *= _nbPlayers;
            index += 1;
            playerChoiceHighlights[index].enabled = true;
        }
    }

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

    public void ClearPlayerChoiceHighlights(){
        foreach(var img in playerChoiceHighlights){
            if (img) img.enabled = false;
        }
    }
    
    private void HighlightTransition(Graphic oldImg, Graphic newImg, bool phaseSelection=false){
        oldImg.CrossFadeAlpha(0f, fadeDuration, false);

        if (phaseSelection) return;
        
        newImg.CrossFadeAlpha(1f, fadeDuration, false);
    }

    // private void PlayerChoiceTransition(Graphic img, bool active)
    // {
    //     if(active) img.CrossFadeAlpha(1f, fadeDuration, false);
    //     else img.CrossFadeAlpha(playerChoiceInactiveAlpha, fadeDuration, false);
    // }
}