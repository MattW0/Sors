using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhaseVisualsUI : MonoBehaviour
{
    public static PhaseVisualsUI Instance { get; private set; }
    private PlayerInterfaceManager _playerInterfaceManager;

    [SerializeField] private List<Image> extendedHighlights;
    [SerializeField] private List<Image> phaseHighlights;

    [SerializeField] private Color phaseHighlightColor;

    [SerializeField] private float fadeDuration = 1f;
    private Image _oldHighlight;
    private Image _newHighlight;

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    public void PrepareUI()
    {
        print("Preparing UI");
        
        GetHighlightImages();
        _oldHighlight = phaseHighlights[0];

        foreach (var img in phaseHighlights)
        {
            img.color = phaseHighlightColor;
            img.CrossFadeAlpha(0f, 0f, true);
        }
    }

    public void UpdatePhaseHighlight(int newHighlightIndex)
    {
        switch (newHighlightIndex) {
            case -1:  // No highlightable phase
                return;
            case -2: // In CleanUp or PhaseSelection
                HighlightTransition(_oldHighlight, null, true);
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

    private void GetHighlightImages()
    {
        // Maybe implement in Unity Editor ?
        var gridTransform = gameObject.transform.GetChild(1);

        foreach (Transform child in gridTransform) {
            extendedHighlights.Add(child.GetComponent<Image>());

            foreach (Transform imgTransform in child)
            {
                phaseHighlights.Add(imgTransform.GetComponent<Image>());
            }
        }
    }
}
