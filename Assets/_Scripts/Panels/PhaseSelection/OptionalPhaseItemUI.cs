using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class OptionalPhaseItemUI : MonoBehaviour, IHighlightable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private TurnState _phase;
    private TooltipWindow _tooltip;
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic _highlight;
    [SerializeField] private Image playerChoice;
    [SerializeField] private Image opponentChoice;
    [SerializeField] private Image tooltipEffect;
    [SerializeField] private Image tooltipBonus;
    private bool _selectable;
    private bool _isSelected;
    public bool IsSelected 
    {
        get => _isSelected;
        set {
            _isSelected = value;
            playerChoice.enabled = value;
            tooltipBonus.enabled = value;
        }
    }

    public bool TooltipDisabled { get; set; }

    public static event Action<TurnState> OnToggleSelection;

    private void Awake()
    {
        _tooltip = GetComponentInChildren<TooltipWindow>();

        PhasePanel.OnPhaseSelectionStarted += StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed += EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed += ShowOpponentSelection;
    }

    private void Start()
    {
        _phase = (TurnState) Enum.Parse(typeof(TurnState), gameObject.name);
        Disable(0.1f);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!_selectable) return;

        IsSelected = !IsSelected;
        OnToggleSelection?.Invoke(_phase);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(TooltipDisabled) return;

        _tooltip.WindowIn();
        if (_selectable) playerChoice.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(TooltipDisabled) return;

        _tooltip.WindowOut();
        if(!_isSelected) playerChoice.enabled = false;
    }

    private void StartSelection()
    {
        _selectable = true;
        IsSelected = false;
        
        opponentChoice.enabled = false;
        tooltipEffect.enabled = false;
        Highlight(1f, 1f);
    }

    private void ShowOpponentSelection(TurnState phase)
    {
        if(phase != _phase) return;
        
        tooltipEffect.enabled = true;
        opponentChoice.enabled = true;
    }

    private void EndSelection()
    {
        _selectable = false;
        Disable(1f);
    }

    private void OnDestroy()
    {
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed -= ShowOpponentSelection;
    }

    public void Highlight(float alpha, float fadeDuration)
    {
        _highlight.CrossFadeAlpha(alpha, fadeDuration, false);
    }

    public void Disable(float fadeDuration)
    {
        _highlight.CrossFadeAlpha(0f, fadeDuration, false);
    }
}