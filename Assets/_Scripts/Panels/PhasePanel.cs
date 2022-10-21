using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhasePanel : NetworkBehaviour
{
    public PhaseItemUI[] phaseItems;

    [Header("Turn screen")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private GameObject overlayObject;
    private Image _backgroundImage;
    
    [SerializeField] private int turnScreenWaitTime = 1;
    [SerializeField] private float turnScreenFadeTime = 0.5f;

    private int _nbActive;
    public bool disableSelection;
    public Button confirm;
    
    private void Awake() {
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);

        _nbActive = 0;
        phaseItems = GetComponentsInChildren<PhaseItemUI>();
    }

    private void Start() {
        turnText.text = $"Turn {GameManager.Instance.turnNb}";
        
        if(!GameManager.Instance.animations) {
            turnScreenWaitTime = 0;
            turnScreenFadeTime = 0f;
        }
        
        // "Background" color
        _backgroundImage = overlayObject.transform.GetChild(0).GetComponent<Image>();
        StartCoroutine(WaitAndFade());
    }

    private IEnumerator WaitAndFade() {
        
        // Wait and fade
        yield return new WaitForSeconds(turnScreenWaitTime);
        _backgroundImage.CrossFadeAlpha(0f, turnScreenFadeTime, false);
        turnText.CrossFadeAlpha(0f, turnScreenFadeTime, false);

        // Wait and disable
        yield return new WaitForSeconds(turnScreenFadeTime);
        overlayObject.SetActive(false);
    }

    public void UpdateActive()
    {
        _nbActive = 0;
        foreach (var phaseItem in phaseItems)
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

        var selectedItems = new List<Phase>();
        var i = 0;
        foreach (var phaseItem in phaseItems)
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