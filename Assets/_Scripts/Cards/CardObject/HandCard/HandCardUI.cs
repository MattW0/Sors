using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using NUnit.Framework;

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

    public void Highlight(HighlightType type)
    {
        // highlight.color = color;
        highlight.enabled = type != HighlightType.None;
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
