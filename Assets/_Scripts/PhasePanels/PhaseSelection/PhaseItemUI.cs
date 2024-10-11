using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Phase _phase;
    [SerializeField] private GameObject _tooltip;
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic outline;
    [SerializeField] private Image playerChoice;
    [SerializeField] private Image opponentChoice;
    public static event Action<Phase> OnToggleSelection;
    private bool _selectable;
    private bool _isSelected;
    private bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            playerChoice.enabled = value;
        }
    }


    private void Start()
    {
        _phase = (Phase) System.Enum.Parse(typeof(Phase), gameObject.name);

        PhasePanel.OnPhaseSelectionStarted += StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed += EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed += ShowOpponentSelection;

        outline.color = SorsColors.phaseHighlight;
    }

    private void StartSelection(int _)
    {
        _selectable = true;
        IsSelected = false;
        opponentChoice.enabled = false;
        outline.CrossFadeAlpha(0.5f, 1f, false);
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

    public void OnPointerClick(PointerEventData data)
    {
        if (!_selectable) return;

        OnToggleSelection?.Invoke(_phase);
        IsSelected = !_isSelected;
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

    private void OnDestroy()
    {
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= EndSelection;
        PhasePanelUI.OnPhaseSelectionConfirmed -= ShowOpponentSelection;
    }
}