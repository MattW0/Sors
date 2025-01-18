using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using DG.Tweening;

public class NonOptionalPhaseItemUI : MonoBehaviour, IHighlightable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsSelectable { get; set; }
    public bool TooltipDisabled { get; set; }

    private TooltipWindow _tooltip;
    [SerializeField] private Image _highlight;
    [SerializeField] private Image _icon;
    private Color _color;
    private Color _colorInverse;
    public static event Action OnPressedCombatButton;

    private void Awake()
    {
        _tooltip = GetComponentInChildren<TooltipWindow>();
        _color = _icon.color;
        _colorInverse = _highlight.color;
    }

    private void Start() => Disable(0.1f);

    public void OnPointerClick(PointerEventData data)
    {
        if (!IsSelectable) return;
        
        Disable(0.1f);
        OnPressedCombatButton?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipDisabled) return;
        _tooltip.WindowIn();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipDisabled) return;
        _tooltip.WindowOut();
    }

    public void Highlight(float alpha, float fadeDuration)
    {
        // Don't need alpha but implementing IHighlightable
        _highlight.DOColor(_color, fadeDuration);
        _icon.DOColor(_colorInverse, fadeDuration);
    }

    public void Disable(float fadeDuration)
    {
        _highlight.DOColor(_colorInverse, fadeDuration);
        _icon.DOColor(_color, fadeDuration);
    }
}
