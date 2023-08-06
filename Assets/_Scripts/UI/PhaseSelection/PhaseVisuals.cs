using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhaseVisuals : MonoBehaviour
{
    public static PhaseVisuals Instance { get; private set; }
    private PlayerInterfaceManager _playerInterfaceManager;

    [Header("Player Settings")]
    private int _nbPlayers;
    [SerializeField] private float playerChoiceInactiveAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Progress Bar")]
    [SerializeField] private Transform progressBar;
    private float[] progressBarCheckpoints = {0.02f, 0.11f, 0.24f, 0.36f, 0.45f, 0.55f, 0.64f, 0.76f, 0.89f};

    [Header("Phase Highlights")]
    [SerializeField] private List<Image> phaseHighlights;
    [SerializeField] private List<Image> playerChoiceHighlights;
    private Image _oldHighlight;
    private Image _newHighlight;

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    public void PrepareUI(int nbPlayers)
    {
        _nbPlayers = nbPlayers;

        // GetHighlightImages();
        _oldHighlight = phaseHighlights[0];
        
        int i = 0;
        foreach (var img in playerChoiceHighlights) {
            if(!img) continue;
            if (i%2 == 0) img.color = ColorManager.playerOne;
            else img.color = ColorManager.playerTwo;
            img.enabled = false;
            i++;
            // PlayerChoiceTransition(img, false);
        }
    }

    public void ShowPlayerChoices(Phase[] phases){
        var i = 0;
        foreach(var phase in phases){

            var index = (int) phase;

            // !!! indexing for 1 player (debug)
            if (_nbPlayers == 1) {
                index = index*2;
                playerChoiceHighlights[index].enabled = true;
                continue;
            } 

            if (i < _nbPlayers) index *= _nbPlayers;
            else index = index*_nbPlayers + 1;

            playerChoiceHighlights[index].enabled = true;
            i++;
        }        
        return;
    }

    public void UpdatePhaseHighlight(int newHighlightIndex)
    {
        switch (newHighlightIndex) {
            case -1:  // No highlightable phase
                return;
            case -2: // In CleanUp or PhaseSelection
                ClearPlayerChoiceHighlights();
                return;
            default:
                progressBar.localScale = new Vector3(progressBarCheckpoints[newHighlightIndex+1], 1f, 1f);
                break;
        }



        _newHighlight = phaseHighlights[newHighlightIndex];
        HighlightTransition(_oldHighlight, _newHighlight);
        _oldHighlight = _newHighlight;
    }

    private void ClearPlayerChoiceHighlights(){
        foreach(var img in playerChoiceHighlights){
            if (img) img.enabled = false;
        }

        progressBar.localScale = new Vector3(progressBarCheckpoints[0], 1f, 1f);
    }
    
    private void HighlightTransition(Graphic oldImg, Graphic newImg, bool phaseSelection=false)
    {
        oldImg.CrossFadeAlpha(0f, fadeDuration, false);

        if (phaseSelection) return;
        
        newImg.CrossFadeAlpha(1f, fadeDuration, false);
    }

    private void PlayerChoiceTransition(Graphic img, bool active)
    {
        if(active) img.CrossFadeAlpha(1f, fadeDuration, false);
        else img.CrossFadeAlpha(playerChoiceInactiveAlpha, fadeDuration, false);
    }
}
