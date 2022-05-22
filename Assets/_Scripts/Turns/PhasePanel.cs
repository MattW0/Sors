using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PhasePanel : NetworkBehaviour
{
    // PhasePanel instance;
    private TurnManager turnManager;
    [SyncVar] public bool isActive = false;
    

    private void Awake() {
        // instance = this;
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        PhasePanelUI.onSelectionConfirmend += PhasesConfirmed;

        turnManager = TurnManager.instance;
    }

    [Server]
    private void PhasesConfirmed() {
        Debug.Log("Phases confirmed");
        RpcConfirmButtonPressed();
    }   

    [Command]
    private void CmdConfirmButtonPressed() {
        // turnManager.Ready();
        RpcConfirmButtonPressed();
    }

    [ClientRpc]
    private void RpcConfirmButtonPressed() {

        foreach (var item in gameObject.GetComponent<PhasePanelUI>().selectedItems){
            string phase = item.GetComponent<PhaseItemUI>().phaseName.text;
            print(phase);
        }
    }

    private void OnDestroy() {
        PhasePanelUI.onSelectionConfirmend -= PhasesConfirmed;
    }
}
