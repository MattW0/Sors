using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PhasePanel : NetworkBehaviour
{
    private TurnManager turnManager;
    public PhaseItemUI[] phaseItems;

    [Header("UI")]
    public static int maxActive = 2;
    private int nbActive;
    public bool disableSelection = false;
    public Button confirm;
    
    private void Awake() {
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        turnManager = TurnManager.Instance;

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
        confirm.interactable = false;

        List<Phase> selectedItems = new List<Phase>();
        int i = 0;
        foreach (PhaseItemUI phaseItem in phaseItems)
        {
            if (phaseItem.isSelected){
                selectedItems.Add((Phase) i); // converting to enum type (defined in TurnManager)
            }
            phaseItem.selectionConfirmed = true;
            i++;
        }

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdPhaseSelection(selectedItems);
    }
}