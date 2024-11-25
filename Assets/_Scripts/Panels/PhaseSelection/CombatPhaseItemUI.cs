using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class CombatPhaseItemUI : MonoBehaviour, IHighlightable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsSelectable { get; set; }
    [SerializeField] private Graphic _highlight;
    [SerializeField] private Graphic _icon;
    private TooltipWindow _tooltip;
    public static event Action OnPressedCombatButton;
    private void Start()
    {
        _tooltip = GetComponentInChildren<TooltipWindow>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!IsSelectable) return;
        
        OnPressedCombatButton?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) => _tooltip.WindowIn();
    public void OnPointerExit(PointerEventData eventData) => _tooltip.WindowOut();

    public void Highlight(float alpha, float fadeDuration)
    {
        _highlight.CrossFadeAlpha(alpha, fadeDuration, false);

        // TODO: How to change icon color accordingly?

    }

    public void Disable(float fadeDuration)
    {
        _highlight.CrossFadeAlpha(0, fadeDuration, false);

        // TODO: How to change icon color accordingly?
        //_icon.CrossFadeColor(Color.white, fadeDuration, false, false);
    }
}
