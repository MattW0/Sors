using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhaseItemUI : MonoBehaviour
{
    [SerializeField] private PhasePanel phasePanel;
    public Phase Phase;
    [SerializeField] private TMP_Text phaseName;
    [SerializeField] private Image outline;
    private bool _isSelected;


    private void Start()
    {
        _isSelected = false;
        outline.enabled = false;
    }

    public void OnClick()
    {
        if (phasePanel.disableSelection) return;
        
        if (_isSelected) {
            outline.enabled = false;
            _isSelected = false;
        } else {
            outline.enabled = true;
            _isSelected = true;
        }

        phasePanel.UpdateActive(this);
    }

    public void Reset(){
        _isSelected = false;
        outline.enabled = false;
    }
}