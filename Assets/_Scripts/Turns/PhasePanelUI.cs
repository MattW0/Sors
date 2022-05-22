using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanelUI : MonoBehaviour
{
    public static int maxActive = 2;
    private int nbActive;
    public PhaseItemUI[] phaseItems;
    public List<PhaseItemUI> selectedItems;
    public bool disableSelection = false;
    public Button confirm;
    public static event Action onSelectionConfirmend; 

    private void Start()
    {
        nbActive = 0;
        phaseItems = GetComponentsInChildren<PhaseItemUI>();
    }

    public void UpdateActive()
    {
        nbActive = 0;
        foreach (PhaseItemUI phaseItem in phaseItems)
        {
            if (phaseItem.isSelected) nbActive++;
        }

        if(nbActive == maxActive) {
            disableSelection = true;
            confirm.interactable = true;
        } else {
            disableSelection = false;
            confirm.interactable = false;
        }
    }

    public void ConfirmButtonPressed(){
        selectedItems = new List<PhaseItemUI>();

        foreach (PhaseItemUI phaseItem in phaseItems)
        {
            if (phaseItem.isSelected) selectedItems.Add(phaseItem);
            phaseItem.selectionConfirmed = true;
        }

        onSelectionConfirmend?.Invoke();
    }
}
