using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfacePhaseVisuals : MonoBehaviour
{
    public static PlayerInterfacePhaseVisuals Instance { get; private set; }
    private PlayerInterfaceManager _playerInterfaceManager;

    private int _nbPlayers;
    [SerializeField] private List<Image> extendedHighlights;
    [SerializeField] private List<Image> phaseHighlights;
    [SerializeField] private List<Image> playerChoicHighlights;

    [SerializeField] private Color phaseHighlightColor;
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

        GetHighlightImages();
        _oldHighlight = phaseHighlights[0];

        foreach (var img in phaseHighlights)
        {
            img.color = phaseHighlightColor;
            img.CrossFadeAlpha(0f, 0f, true);
        }
    }

    public void ShowPlayerChoices(Phase[] phases){

        var i = 0;
        foreach(var phase in phases){
            var index = (int) phase;

            // !!! indexing for 2 players
            if (i < 2) index *= 2;
            else index = index*2 + 1;

            playerChoicHighlights[index].enabled = true;
            i++;
        }        
        return;
    }

    private void ClearPlayerChoicHighlights(){
        foreach(var img in playerChoicHighlights){
            if (img) img.enabled = false;
        }
    }

    public void UpdatePhaseHighlight(int newHighlightIndex)
    {
        switch (newHighlightIndex) {
            case -1:  // No highlightable phase
                return;
            case -2: // In CleanUp or PhaseSelection
                HighlightTransition(_oldHighlight, null, true);
                ClearPlayerChoicHighlights();
                return;
        }
        _newHighlight = phaseHighlights[newHighlightIndex];
        HighlightTransition(_oldHighlight, _newHighlight);
        _oldHighlight = _newHighlight;
    }
    
    private void HighlightTransition(Graphic oldImg, Graphic newImg, bool phaseSelection=false)
    {
        oldImg.CrossFadeAlpha(0f, fadeDuration, false);

        if (phaseSelection) return;
        
        newImg.CrossFadeAlpha(1f, fadeDuration, false);
    }

    // Maybe implement in Unity Editor ?
    private void GetHighlightImages()
    {
        var gridTransform = gameObject.transform.GetChild(1);

        foreach (Transform child in gridTransform) {
            extendedHighlights.Add(child.GetComponent<Image>());

            foreach (Transform imgTransform in child)
            {
                phaseHighlights.Add(imgTransform.GetComponent<Image>());

                if (child.name == "Combat") {
                    // Accounting for Combat in Phases definition
                    playerChoicHighlights.Add(null);
                    playerChoicHighlights.Add(null);
                    continue;
                }

                var playerChoices = imgTransform.GetChild(1);
                foreach (Transform playerChoice in playerChoices)
                {
                    playerChoicHighlights.Add(playerChoice.GetComponent<Image>());
                }
            }
        }
    }
}
