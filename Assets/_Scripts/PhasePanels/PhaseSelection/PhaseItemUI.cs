using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using Unity.Multiplayer.Center.Common;

public class PhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Phase _phase;
    private TooltipWindow _tooltip;
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic outline;
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
    public static event Action<Phase> OnToggleSelection;

    private void Start()
    {
        _phase = (Phase) Enum.Parse(typeof(Phase), gameObject.name);
        _tooltip = GetComponentInChildren<TooltipWindow>();

        PhasePanel.OnPhaseSelectionStarted += StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed += EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed += ShowOpponentSelection;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!_selectable) return;

        IsSelected = !IsSelected;
        OnToggleSelection?.Invoke(_phase);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.WindowIn();
        if (_selectable) playerChoice.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.WindowOut();
        if(!_isSelected) playerChoice.enabled = false;
    }

    private void StartSelection()
    {
        _selectable = true;
        IsSelected = false;
        
        opponentChoice.enabled = false;
        tooltipEffect.enabled = false;
        outline.CrossFadeAlpha(0.5f, 1f, false);
    }

    private void ShowOpponentSelection(Phase phase)
    {
        if(phase != _phase) return;
        
        tooltipEffect.enabled = true;
        opponentChoice.enabled = true;
    }

    private void EndSelection()
    {
        _selectable = false;
        outline.CrossFadeAlpha(0f, 1f, false);
    }

    private void OnDestroy()
    {
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed -= ShowOpponentSelection;
    }
}