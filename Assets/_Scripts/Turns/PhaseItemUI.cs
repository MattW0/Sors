using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhaseItemUI : MonoBehaviour
{
    public TMP_Text phaseName;
    public Image outline;
    public bool isSelected = false;
    public bool selectionConfirmed = false;
    public PhasePanelUI phaseUI;

    void Start()
    {
        outline.enabled = false;
    }

    public void OnClick()
    {
        if (selectionConfirmed) return;
        
        if (outline.enabled) {
            outline.enabled = false;
            isSelected = false;
        } else if (!phaseUI.disableSelection) {
            outline.enabled = true;
            isSelected = true;
        }

        phaseUI.UpdateActive();
    }
}
