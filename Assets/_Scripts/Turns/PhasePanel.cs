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
    [SerializeField] private GameObject overlayObject;
    [SerializeField] private int turnScreenWaitTime = 1;
    [SerializeField] private float turnScreenFadeTime = 0.5f;

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
        StartCoroutine(WaitAndFade());
    }

    private IEnumerator WaitAndFade() {
        
        // "Background" color
        Image _sprite = overlayObject.transform.GetChild(0).GetComponent<Image>();

        // Wait and fade
        yield return new WaitForSeconds(turnScreenWaitTime);
        _sprite.CrossFadeAlpha(0f, turnScreenFadeTime, false);
        _turnText.CrossFadeAlpha(0f, turnScreenFadeTime, false);

        // Wait and disable
        yield return new WaitForSeconds(turnScreenFadeTime);
        overlayObject.SetActive(false);
    }

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