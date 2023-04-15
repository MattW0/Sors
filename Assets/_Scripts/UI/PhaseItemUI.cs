using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PhaseItemUI : MonoBehaviour, IPointerClickHandler
{
    private PhasePanel _phasePanel;
    private Phase _phase;
    [SerializeField] private Graphic outline;
    [SerializeField] private Color phaseHighlightColor = new Color32(147, 147, 147, 255);
    [SerializeField] private Color phaseSelectedColor = new Color32(150, 100, 200, 255);

    private bool _selectable;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set{
            _isSelected = value;
            if(_isSelected) outline.color = phaseSelectedColor;
            else outline.color = phaseHighlightColor;
        }
    }

    private void Start(){
        _phasePanel = PhasePanel.Instance;
        _phase = (Phase) System.Enum.Parse(typeof(Phase), gameObject.name);

        outline.color = phaseHighlightColor;

        PhasePanel.OnPhaseSelectionStarted += StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed += Reset;
    }

    private void StartSelection(){
        _selectable = true;
        outline.CrossFadeAlpha(1f, 0f, false);
    }

    public void OnPointerClick(PointerEventData data){
        if (!_selectable) return;
        IsSelected = !_isSelected;
        _phasePanel.UpdateActive(_phase);
    }

    private void Reset(){
        _selectable = false;
        IsSelected = false;

        outline.CrossFadeAlpha(0f, 1f, false);
    }

    private void OnDestroy(){
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= Reset;
    }
}