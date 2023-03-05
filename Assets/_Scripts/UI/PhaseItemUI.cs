using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhaseItemUI : MonoBehaviour
{
    private PhasePanel _phasePanel;
    private Phase _phase; // assigned in editor
    [SerializeField] private TMP_Text phaseName;
    [SerializeField] private Image outline;
    private bool _isSelected;


    private void Start()
    {
        // Only do this once
        if (_phasePanel) return;

        _phasePanel = PhasePanel.Instance;
        _phase = (Phase) System.Enum.Parse(typeof(Phase), gameObject.name);
        PhasePanel.OnPhaseSelectionEnded += Reset;
    }

    public void OnClick()
    {
        if (_phasePanel.disableSelection) return;
        
        if (_isSelected) {
            outline.enabled = false;
            _isSelected = false;
        } else {
            outline.enabled = true;
            _isSelected = true;
        }

        _phasePanel.UpdateActive(_phase);
    }

    public void Reset(){
        _isSelected = false;
        outline.enabled = false;
    }

    private void OnDestroy(){
        PhasePanel.OnPhaseSelectionEnded -= Reset;
    }
}