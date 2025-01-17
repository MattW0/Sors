using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class PhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Phase _phase;
    [SerializeField] private GameObject _tooltip;
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic outline;
    [SerializeField] private Image playerChoice;
    [SerializeField] private Image opponentChoice;
    private bool _selectable;
    private bool _isSelected;
    public static event Action<Phase> OnToggleSelection;

    private void Start()
    {
        _phase = (Phase) Enum.Parse(typeof(Phase), gameObject.name);

        PhasePanel.OnPhaseSelectionStarted += StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed += EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed += ShowOpponentSelection;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!_selectable) return;

        OnToggleSelection?.Invoke(_phase);
        SetSelected(!_isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.SetActive(true);
        playerChoice.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.SetActive(false);
        if(!_isSelected) playerChoice.enabled = false;
    }

    private void StartSelection(int _)
    {
        _selectable = true;
        SetSelected(false);
        
        opponentChoice.enabled = false;
        outline.CrossFadeAlpha(0.5f, 1f, false);
    }

    private void SetSelected(bool v)
    {
        _isSelected = v;
        playerChoice.enabled = v;
    }

    private void ShowOpponentSelection(Phase phase)
    {
        if(phase == _phase) opponentChoice.enabled = true;
    }

    private void EndSelection()
    {
        _selectable = false;
        outline.CrossFadeAlpha(0f, 1f, false);
        if(_tooltip) _tooltip.SetActive(false);
    }

    private void OnDestroy()
    {
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed -= ShowOpponentSelection;
    }
}