using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhaseVisualsUI : MonoBehaviour
{
    [SerializeField] private Image[] extendedHighlights;
    [SerializeField] private Image[] phaseHighlights;
    [SerializeField] private Image[] combatHighlights;

    [SerializeField] private Color phaseHighlightColor;
    [SerializeField] private Color combatHighlightColor;

    [SerializeField] private float fadeDuration = 1f;
    private Image _oldHighlight;
    private Image _newHighlight;
    
    private void Awake()
    {
        _oldHighlight = phaseHighlights[0];
        TurnManager.OnPhaseChanged += UpdatePhaseHighlight;
    }

    private void UpdatePhaseHighlight(TurnState newState)
    {
        print("TurnChange");
        _newHighlight = newState switch
        {
            TurnState.DrawI => phaseHighlights[0],
            TurnState.Develop => phaseHighlights[1],
            TurnState.Deploy => phaseHighlights[2],
            TurnState.DrawII => phaseHighlights[3],
            TurnState.Recruit => phaseHighlights[4],
            TurnState.Prevail => phaseHighlights[5],
            _ => _newHighlight
        };

        HighlightTransition(_oldHighlight, _newHighlight);
        _oldHighlight = _newHighlight;
    }
    
    private void HighlightTransition(Graphic oldImg, Graphic newImg)
    {
        var oldCol = oldImg.color;
        oldCol.a = 0;
        
        var newCol = phaseHighlightColor;
        newCol.a = 1;

        StartCoroutine(ColorLerp(oldImg, oldCol, fadeDuration));
        StartCoroutine(ColorLerp(newImg, newCol, fadeDuration));
    }

    private static IEnumerator ColorLerp(Graphic img, Color endValue, float duration)
    {
        float time = 0;
        var startValue = img.color;
        while (time < duration)
        {
            img.color = Color.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        img.color = endValue;
    }

    private void OnDestroy()
    {
        TurnManager.OnPhaseChanged -= UpdatePhaseHighlight;
    }
}
