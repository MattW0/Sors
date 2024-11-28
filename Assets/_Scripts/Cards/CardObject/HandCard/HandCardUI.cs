using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HandCardUI : CardUI, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _front;
    private WaitForSeconds _wait = new(SorsTimings.hoverPreviewDelay);
    private float _offset = 0.2f;

    public bool IsHovered { get; private set; }


    public void CardBackUp()
    {
        _front.SetActive(false);
    }
    public void CardFrontUp()
    {
        _front.SetActive(true);
    }

    public void Highlight(bool value, TurnState state)
    {
        if (!value || state == TurnState.None) {
            highlight.enabled = false;
            return;
        }

        var color = state switch
        {
            // TurnState.Develop or TurnState.Deploy => ColorPalette.interactionPositiveHighlight,
            TurnState.Trash or TurnState.Discard => UIManager.ColorPalette.interactionNegativeHighlight,
            TurnState.CardSelection => UIManager.ColorPalette.interactionPositiveHighlight,
            _ => UIManager.ColorPalette.defaultHighlight
        };

        highlight.color = color;
        highlight.enabled = true;
    }

    public void Highlight(HighlightType type)
    {
        if (type == HighlightType.None)
        {
            highlight.enabled = false;
            return;
        }

        highlight.color = type switch
        {
            HighlightType.Playable => UIManager.ColorPalette.interactionPositiveHighlight,
            HighlightType.Selected => UIManager.ColorPalette.defaultHighlight,
            _ => UIManager.ColorPalette.defaultHighlight
        };
        highlight.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        return; 
        StopAllCoroutines();
        StartCoroutine(HoverDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        return;
        StopAllCoroutines();
        if (!IsHovered) return;

        transform.DOMoveZ(transform.position.z - _offset, 0.2f);
        transform.DOMoveX(transform.position.x - _offset, 0.2f);
        transform.DOScale(1f, 0.2f);

        IsHovered = false;
    }

    private IEnumerator HoverDelay()
    {
        yield return _wait;

        transform.DOMoveZ(transform.position.z + _offset, 0.2f);
        transform.DOMoveX(transform.position.x + _offset, 0.2f);
        transform.DOScale(1.2f, 0.2f);
        
        IsHovered = true;
    }
}
