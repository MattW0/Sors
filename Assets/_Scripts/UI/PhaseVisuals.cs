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

    private int _nbPlayers;
    [SerializeField] private List<Image> extendedHighlights;
    [SerializeField] private List<Image> phaseHighlights;
    [SerializeField] private List<Image> playerChoiceHighlights;
    [SerializeField] private float playerChoiceInactiveAlpha = 0.3f;
    [SerializeField] private Color playerColor1;
    [SerializeField] private Color playerColor2;
    [SerializeField] private float fadeDuration = 1f;
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
            if (i%2 == 0) img.color = playerColor1;
            else img.color = playerColor2;
            img.enabled = false;
            i++;
            // PlayerChoiceTransition(img, false);
        }
    }

    public void ShowPlayerChoices(Phase[] phases){

        print("ShowPlayerChoices");

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
        }
        _newHighlight = phaseHighlights[newHighlightIndex];
        HighlightTransition(_oldHighlight, _newHighlight);
        _oldHighlight = _newHighlight;
    }

    private void ClearPlayerChoiceHighlights(){
        foreach(var img in playerChoiceHighlights){
            if (img) img.enabled = false;
        }
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
