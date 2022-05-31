using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasePanel : NetworkBehaviour
{
    private TurnManager turnManager;
    public PhaseItemUI[] phaseItems;

    [Header("Turn screen")]
    [SerializeField] private TMP_Text _turnText;
    [SerializeField] private GameObject overlayImage;
    [SerializeField] private float turnScreenWaitTime = 2f;
    [SerializeField] private float turnScreenFadeTime = 1f;

    private int _nbActive;
    public bool disableSelection;
    public Button confirm;
    
    private void Awake() {
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        turnManager = TurnManager.Instance;

        _nbActive = 0;
        phaseItems = GetComponentsInChildren<PhaseItemUI>();
    }

    private void Start() {
        _turnText.text = $"Turn {GameManager.Instance.turnNb}";
        // WaitAndFade();
        overlayImage.GetComponent<Image>().CrossFadeAlpha(0f, turnScreenFadeTime, false);
        _turnText.CrossFadeAlpha(0f, turnScreenFadeTime, false);
        overlayImage.SetActive(false);
    }

    // private IEnumerator WaitAndFade() {
    //     yield return new WaitForSeconds(turnScreenWaitTime);
    // }

    public void UpdateActive()
    {
        _nbActive = 0;
        foreach (PhaseItemUI phaseItem in phaseItems)
        {
            if (phaseItem.isSelected) _nbActive++;
        }

        if(_nbActive == GameManager.Instance.nbPhasesToChose) {
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